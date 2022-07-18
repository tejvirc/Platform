﻿namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using log4net;

    /// <summary>
    ///     The handler for implied ACKs from SAS
    /// </summary>
    public class HostAcknowledgementProvider : IHostAcknowlegementProvider
    {
        private enum SyncState
        {
            PendingAnotherAddress,
            PendingFirstMessage,
            Synchronized
        }

        private const int CommandCompareLength = 2; // We only need to check up to the command byte in a long poll
        //EFT requirements are from section 8.EFT of the SAS v5.02 document  - https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
        private const int CompareLengthMultiDenom = 5;
        private const int FinalNackCount = 2; // We have a final NACK count of 2 as SAS will repeat the same message 3 times (2 NACKs)
        private const int FinalNackCountEft = 8; // We have a final NACK count of 8 for EFT as SAS will repeat the same message 9 times
        private const int ImpliedAckTimeout = 30_000; // 30 second timeout per the SAS Specification

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object _lockObject = new object();
        private Timer _impliedAckTimer;

        private bool _disposed;
        private bool _pendingImpliedAck;
        private SyncState _syncState = SyncState.PendingAnotherAddress;
        private int _impliedNackCount;
        private int _compareLength = CommandCompareLength;
        private IReadOnlyCollection<byte> _lastMessage = new List<byte>();

        /// <summary>
        ///     Creates the implied ACK handler
        /// </summary>
        /// <param name="timer">timer for unit tests</param>
        public HostAcknowledgementProvider(Timer timer = null)
        {
            _impliedAckTimer = timer ?? new Timer(ImpliedAckTimeout);
            _impliedAckTimer.Elapsed += ImpliedAckTimerOnElapsed;
            _impliedAckTimer.AutoReset = false; // If we timeout we will reset the timer
        }

        /// <inheritdoc />
        public event EventHandler SynchronizationLost;

        /// <inheritdoc />
        public bool Synchronized => _syncState == SyncState.Synchronized;

        /// <inheritdoc />
        public bool LastMessageNacked => _impliedNackCount != 0 && Synchronized;

        /// <inheritdoc />
        public void LinkDown()
        {
            lock (_lockObject)
            {
                Logger.Warn(
                    $"A Link Down when pendingImpliedAck = {_pendingImpliedAck} and impliedNackCount = {_impliedNackCount}");
                _syncState = SyncState.PendingAnotherAddress;
                _impliedAckTimer?.Stop();

                // If we receive a linked down this is an implied NACK
                ImpliedNack();
            }
        }

        /// <inheritdoc />
        public bool CheckImpliedAck(bool globalBroadcast, bool otherAddressPoll, IReadOnlyCollection<byte> data)
        {
            lock (_lockObject)
            {
                var synchronized = Synchronized;
                // If we are not waiting on an implied ack or we are currently not synchronized no need to check for an implied ACK or NACK
                if (!_pendingImpliedAck || !synchronized)
                {
                    if (otherAddressPoll || globalBroadcast)
                    {
                        _impliedNackCount = 0;
                        _pendingImpliedAck = false;
                        _syncState = SyncState.PendingFirstMessage;
                        Logger.Debug("Sync poll has been received");
                    }
                    else if (_syncState == SyncState.PendingFirstMessage) // Wait until we addressed
                    {
                        // We are going to be synchronized now so clear the timer and start processing polls
                        // We don't want to declare being synchronized until we get at least one poll for our machine
                        ClearImpliedAckTimeOut();
                        return true;
                    }

                    return synchronized;
                }

                // This table was taken from Section 3.1 and regrouped based on poll type and not the implied ACK.
                // --------------------------------------------------------------------------------------------------------------
                //      Poll To Ack          |                                  Implied Ack
                // --------------------------------------------------------------------------------------------------------------
                //        General            |  Any Long poll, global poll, or general poll to any other gaming machine address
                // --------------------------------------------------------------------------------------------------------------
                //         Long              |  Any Long poll with a different command, global poll, any general poll, or
                //                           |                    long poll to any other gaming machine address
                // --------------------------------------------------------------------------------------------------------------
                //      EFT Long Poll        |  Any Long poll with a different transaction number is considered an Implied ACK
                //   *not included in doc    |   as an EFT message has two phases, an initial message with ACK 0 and a another
                //                           |                          message following with ACK 1.
                //                           |  The EFT Nack/Resend count is increased to 9 from the SAS default of 3 but only
                //                           |                  applies to acknowledgment messages (ACK = 1)
                // --------------------------------------------------------------------------------------------------------------

                if (globalBroadcast || // Any global broadcast message is always an implied ACK
                        !data.Take(_compareLength).SequenceEqual(_lastMessage.Take(_compareLength))) // Check the first data points to see if we have the same command
                {
                    ImpliedAck();
                }
                else
                {
                    //If incoming message is an EFT command, check if Ack flag is same as last message
                    //If Ack is changed, its not a INack scenario.
                    if (CheckIfEftTransactionCommand(data))
                    {
                        // If it is not re-ack scenario, either its second phase message with ack==1 after first phase message
                        // or it is an invalid ack scenario. In both cases, it should be handled by relevant LP handler.
                        if (!IsReAckReceivedForEftCommands(data, _lastMessage))
                        {

                            //Reset the implied ack as the message should be handled by LP handler
                            _impliedNackCount = 0;
                            return true;
                        }

                        if (++_impliedNackCount >= FinalNackCountEft)
                        {
                            ImpliedNack();
                            Logger.Warn("Final NACK has been received from SAS EFT Long Poll");
                            return false;
                        }
                        else
                        {
                            //Reset the timer for EFT state controller by issuing Re-Ack
                            IntermediateNackHandler?.Invoke();
                        }
                    }
                    else if (++_impliedNackCount >= FinalNackCount)
                    {
                        // We are on the final NACK we don't need to process the message as SAS is telling us it failed
                        // We don't need to process the message as SAS will ignore any response we send
                        ImpliedNack();
                        Logger.Warn("Final NACK has been received from SAS");
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        ///     Checks whether the received command is an EFT transaction command.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True if it is an EFT transaction command.</returns>
        private static bool CheckIfEftTransactionCommand(IReadOnlyCollection<byte> data)
        {
            if (data.Count < CommandCompareLength)
            {
                return false;
            }

            return ((LongPoll)data.ElementAt(SasConstants.SasCommandIndex)).IsEftTransactionLongPoll();
        }

        private static bool IsReAckReceivedForEftCommands(IReadOnlyCollection<byte> data, IReadOnlyCollection<byte> previousMessage)
        {
            var ackIndex = ((LongPoll)data.ElementAt(SasConstants.SasCommandIndex)) == LongPoll.EftSendLastCashOutCreditAmount
                ? 2 : SasConstants.EftAckByteIndex;

            return Convert.ToBoolean(data.ElementAt(ackIndex)) && data.ElementAt(ackIndex).Equals(previousMessage.ElementAt(ackIndex));
        }

        /// <inheritdoc />
        public void SetPendingImpliedAck(IReadOnlyCollection<byte> data, IHostAcknowledgementHandler handlers)
        {
            lock (_lockObject)
            {
                // set the callbacks
                ImpliedAckHandler = handlers?.ImpliedAckHandler;
                ImpliedNackHandler = handlers?.ImpliedNackHandler;
                IntermediateNackHandler = handlers?.IntermediateNackHandler;

                // Set the message to use for the pending implied ACK
                _lastMessage = data;
                _compareLength =
                    data.Count >= SasConstants.MinimumBytesForLongPoll &&
                    data.ElementAt(SasConstants.SasCommandIndex) == (byte)LongPoll.MultiDenominationPreamble
                        ? CompareLengthMultiDenom
                        : CommandCompareLength;
                _pendingImpliedAck = true;
            }
        }

        /// <inheritdoc />
        public Action ImpliedNackHandler { get; set; }

        /// <inheritdoc />
        public Action IntermediateNackHandler { get; set; }

        /// <inheritdoc />
        public Action ImpliedAckHandler { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Handles disposing the object
        /// </summary>
        /// <param name="disposing">Whether or not to dispose the resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            if (_impliedAckTimer != null)
            {
                var timer = _impliedAckTimer;
                _impliedAckTimer = null;
                timer.Stop();
                timer.Elapsed -= ImpliedAckTimerOnElapsed;
                timer.Dispose();
            }

            _disposed = true;
        }

        private void ClearImpliedAckTimeOut()
        {
            if (_impliedAckTimer != null)
            {
                _impliedAckTimer.Interval = ImpliedAckTimeout;
                _impliedAckTimer.Start();
            }

            _syncState = SyncState.Synchronized;
        }

        private void ImpliedAck()
        {
            _pendingImpliedAck = false;
            _impliedNackCount = 0;

            ImpliedAckHandler?.Invoke();
            ImpliedAckHandler = null;
            ImpliedNackHandler = null;
            ClearImpliedAckTimeOut();
        }

        private void ImpliedNack()
        {
            _pendingImpliedAck = false;
            _impliedNackCount = 0;

            ImpliedNackHandler?.Invoke();
            ImpliedAckHandler = null;
            ImpliedNackHandler = null;

            _lastMessage = Array.Empty<byte>();
        }

        private void ImpliedAckTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                const int secondsInMilliseconds = 1_000;
                Logger.Warn(
                    $"No implied ACKs from SAS have been received for more than {ImpliedAckTimeout / secondsInMilliseconds} seconds");
                _syncState = SyncState.PendingAnotherAddress;
                SynchronizationLost?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}