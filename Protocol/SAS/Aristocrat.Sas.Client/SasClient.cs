namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using log4net;
    using LPParsers;
    
    /// <summary>
    ///     This class is responsible for communicating between the comm ports and the SAS LongPoll parsers and
    ///     real time event consumers.
    /// </summary>
    public class SasClient : ISasClient, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly IHostAcknowledgementHandler NullHandlers = new HostAcknowledgementHandler();
        private const int LpWithCrcLength = 4;
        private const int MaximumContinuousWakeups = 10;

        private readonly ISasCommPort _port = new SerialComm.CommPort();
        private readonly ISasExceptionQueue _exceptionQueue;
        private readonly ISasMessageQueue _messageQueue;
        private readonly ISasParserFactory _factory;
        private readonly IHostAcknowlegementProvider _hostAcknowlegementProvider;
        private readonly IPlatformCallbacks _callbacks;
        private readonly List<byte> _readData = new List<byte>();

        private readonly Stopwatch _stopWatch = new Stopwatch();
        private IReadOnlyCollection<byte> _lastMessage = new List<byte>();
        private bool _running = true;
        private bool _disposed;
        public BlockingCollection<SasPollData> SasPollDataBlockingCollection = new BlockingCollection<SasPollData>();

        /// <summary>
        ///     Instantiates a new instance of the SasClient class
        /// </summary>
        /// <param name="configuration">The configuration information for this client</param>
        /// <param name="callbacks">The callbacks into the platform</param>
        /// <param name="exceptionQueue">The exception queue for this client</param>
        /// <param name="messageQueue">The delayed message queue for this client</param>
        /// <param name="factory">The long poll parser factory for this client</param>
        /// <param name="hostAcknowlegementProvider">The implied ack handler</param>
        public SasClient(
            SasClientConfiguration configuration,
            IPlatformCallbacks callbacks,
            ISasExceptionQueue exceptionQueue,
            ISasMessageQueue messageQueue,
            ISasParserFactory factory,
            IHostAcknowlegementProvider hostAcknowlegementProvider)
        {
            Configuration = configuration;
            _exceptionQueue = exceptionQueue ?? throw new ArgumentNullException(nameof(exceptionQueue));
            _messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _hostAcknowlegementProvider = hostAcknowlegementProvider ?? throw new ArgumentNullException(nameof(hostAcknowlegementProvider));
            _callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));

            _port.SasAddress = configuration.SasAddress;
            ClientNumber = configuration.ClientNumber;
            _hostAcknowlegementProvider.SynchronizationLost += HandleLinkDown;
            Diagnostics = new SasDiagnostics(this);
            PerformanceAuditor = Diagnostics.PerformanceTiming;
            Logger.Debug($"[SAS] client {ClientNumber} created.");
        }

        public SasDiagnostics Diagnostics { get; set; }

        /// <summary> Gets the state reflecting the status of receiving and processing data </summary>
        public SasPerformanceTiming PerformanceAuditor { get; }

        /// <inheritdoc />
        public byte ClientNumber { get; }

        /// <inheritdoc/>
        public bool LinkUp { get; private set; }

        /// <summary> Sets or gets a value indicating whether a poll is available </summary>
        public bool IsPollAvailable { set; get; }

        /// <summary> Sets or gets a value indicating whether the poll is a general poll </summary>
        public bool IsGeneralPoll { set; get; }

        /// <summary> Sets or gets a value indicating whether the poll is a long poll </summary>
        public bool IsLongPoll { private set; get; }

        /// <summary> Sets or gets a value indicating whether the poll is a global poll </summary>
        public bool IsGlobalPoll { private set; get; }

        /// <summary> Sets or gets a value indicating whether the poll is a poll for other address </summary>
        public bool IsOtherAddressPoll { private set; get; }

        /// <summary> Sets or gets the long poll command. </summary>
        public byte Command { private set; get; }

        /// <summary> Sets or gets a value indicating whether logging chirps can be started </summary>
        public bool StartLogChirp { set; get; }

        /// <summary>Indicates whether real time event reporting is active or not</summary>
        public bool IsRealTimeEventReportingActive { set; get; }

        /// <inheritdoc/>
        public void SendResponse(IReadOnlyCollection<byte> command)
        {
            GetTicksInHandlingPoll();
            _port.SendRawBytes(command);
            _lastMessage = new List<byte>(command);
        }

        /// <inheritdoc/>
        public bool AttachToCommPort(string portName)
        {
            _callbacks.ToggleCommunicationsEnabled(true, ClientNumber);
            return _port.Open(portName);
        }

        /// <inheritdoc/>
        public void ReleaseCommPort()
        {
            _callbacks.ToggleCommunicationsEnabled(false, ClientNumber);
            _port.Close();
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            // connect the Real Time Event Parser up
            _factory.LoadSingleParser(new LP0ERealTimeEventReportingParser(this));
            // load the multi-denom preamble parser, feeding in our factory
            _factory.LoadSingleParser(new LPB0MultiDenomPreambleParser(_factory));
        }

        /// <inheritdoc/>
        public void Run()
        {
            // mark that the link is down until we get something from the comm port.
            _callbacks.LinkUp(false, ClientNumber);
            Diagnostics.CommsStatus = CommsStatus.Offline;
            Diagnostics.LastLinkDown = DateTime.Now.ToString(CultureInfo.CurrentCulture);

            PerformanceAuditor.ChirpWatch.Restart();
            PerformanceAuditor.LogChirpWatch.Restart();

            // Wait for sas message notification and send the message to the appropriate handler
            // if it's a general poll send any events in the exceptionQueue
            while (_running)
            {
                PeekPoll();
                var canProcess = IsPollAvailable && _hostAcknowlegementProvider.CheckImpliedAck(IsGlobalPoll, IsOtherAddressPoll, _readData);
                UpdateLinkDownStatus();

                if (!canProcess)
                {
                    Diagnostics.BadPacket++;
                    continue;
                }

                if (_hostAcknowlegementProvider.LastMessageNacked)
                {
                    ResendLastMessage();
                }
                else
                {
                    OnPollReceived();
                }
            }

            ReleaseCommPort();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            _running = false;
        }

        public SasClientConfiguration Configuration { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_hostAcknowlegementProvider != null)
                {
                    _hostAcknowlegementProvider.SynchronizationLost -= HandleLinkDown;
                    _hostAcknowlegementProvider.Dispose();
                }

                SasPollDataBlockingCollection.Dispose();
            }

            _disposed = true;
        }

        private void OnPollReceived()
        {
            // at this point we have one of the following:
            //  - we have a general poll (0x80 | our SAS address)
            //  - we have a synchronize poll (global poll or another address poll)
            //  - we have a long poll (our SAS address)
            if ((IsGlobalPoll && !IsLongPoll) || IsOtherAddressPoll)
            {
                HandleSynchronizePoll();
            }
            else if (IsGeneralPoll)
            {
                HandleGeneralPoll();
            }
            else if (IsLongPoll)
            {
                ProcessLongPollMessage();
            }

            Diagnostics.Ack++;
            PerformanceAuditor.TicksUsedTotally = _stopWatch.ElapsedTicks;
            PerformanceAuditor.PrintAudit(Logger);
        }

        private void HandleGeneralPoll()
        {
            // For general polls we have the following response options:
            //   - if we have a pending message from a long running command (ROM Signature request for example) send the message
            //   - if we are in Real Time Event reporting mode and we have an exception pending, send the exception
            //   - if we are in Real Time Event reporting mode with no pending exceptions, ignore the general poll
            //   - if we are not in Real Time Event reporting mode, report the next exception or NONE.
            var elapsed = _stopWatch.ElapsedTicks;
            PerformanceAuditor.TicksToPeekAtTopException = _stopWatch.ElapsedTicks - elapsed;
            var handlers = NullHandlers;

            // Before checking the exception queue, see if there are any responses for long running commands. These should 
            // be handled first
            if (!_messageQueue.IsEmpty)
            {
                var nextMessage = _messageQueue.GetNextMessage();
                if (nextMessage.MessageData.Count > 0)
                {
                    // set messageQueue as target of next ACK/NACK
                    handlers.ImpliedAckHandler = _messageQueue.MessageAcknowledged;
                    handlers.ImpliedNackHandler = _messageQueue.ClearPendingMessage;
                    var response = Utilities.CalculateAndAddCrc(nextMessage.MessageData.ToArray());
                    ProduceSasPollData(
                        SasPollData.PacketType.Tx,
                        SasPollData.PollType.LongPoll,
                        response,
                        _stopWatch.ElapsedTicks - PerformanceAuditor.TicksToGetPoll);
                    SendResponse(response);
                }
                else
                {
                    GetTicksInHandlingPoll();

                    // This should never happen and if this does we have a bug
                    Logger.Error("[SAS] We have data in the message queue but the message length is zero");
                }
            }
            else
            {
                // No more long running messages to take care of, now look at the exception queue
                var pendingException = _exceptionQueue.GetNextException();
                var exceptionCode = _exceptionQueue.ConvertRealTimeExceptionToNormal(pendingException);
                List<byte> response;
                if (IsRealTimeEventReportingActive)
                {
                    Logger.Debug("[SAS] Reporting long form real time exception");
                    response = new List<byte> { _port.SasAddress, (byte)LongPoll.RealTimeEventResponseToLongPoll };
                    response.AddRange(pendingException);
                    response = Utilities.CalculateAndAddCrc(response.ToArray()).ToList();
                    GetTicksInHandlingPoll();
                }
                // If not doing Real Time Event Reporting then send next exception byte or NONE
                else
                {
                    Logger.Debug("[SAS] Sending one byte exception");
                    PerformanceAuditor.TicksToGetNextException = _stopWatch.ElapsedTicks - elapsed;
                    response = new List<byte>(new[] { (byte)exceptionCode });
                }
                ProduceSasPollData(
                    SasPollData.PacketType.Tx,
                    exceptionCode == GeneralExceptionCode.None
                        ? SasPollData.PollType.NoActivity
                        : SasPollData.PollType.GeneralPoll,
                    response,
                    _stopWatch.ElapsedTicks - PerformanceAuditor.TicksToGetPoll);
                SendResponse(response);
                PerformanceAuditor.TicksToSendBytes = _stopWatch.ElapsedTicks - elapsed;

                // set exceptionQueue as the next ACK/NACK target
                handlers.ImpliedAckHandler = _exceptionQueue.ExceptionAcknowledged;
                handlers.ImpliedNackHandler = _exceptionQueue.ClearPendingException;

                if (exceptionCode == GeneralExceptionCode.EePromDataError)
                {
                    Logger.Info("[SAS] reporting the critical memory error; SAS address is set to 0 and the communication with the backend is stopped.");

                    // no need to re-assign it but the spec says it defaults to zero when it happens.                
                    _port.SasAddress = 0;
                    _callbacks.LinkUp(false, ClientNumber);

                    // stop it to ignore all communication.
                    Stop();
                }
            }

            // If we have a general poll we always need an implied ACK
            _hostAcknowlegementProvider.SetPendingImpliedAck(new List<byte>(_readData), handlers);
        }

        private void ProduceSasPollData(
            SasPollData.PacketType type,
            SasPollData.PollType pollType,
            IReadOnlyCollection<byte> pollData,
            decimal elapsedTicks)
        {
            if (pollData == null)
            {
                return;
            }

            try
            {
                var ticksAlreadySpent = _stopWatch.ElapsedTicks;
                var millisecondsPerTick = 1_000M / Stopwatch.Frequency;
                
                SasPollDataBlockingCollection.Add(
                    new SasPollData(pollData) { Type = type, SasPollType = pollType, ElapsedTime = elapsedTicks });
                Diagnostics.TotalTimeTakenToPopulateDiagnostics = (_stopWatch.ElapsedTicks - ticksAlreadySpent) * millisecondsPerTick;

                Logger.Debug($"Total time taken to populate SAS diagnostics: {(_stopWatch.ElapsedTicks - ticksAlreadySpent) * millisecondsPerTick}ms");
            }
            catch (ObjectDisposedException)
            {
                Logger.Debug("SasPollDataBlockingCollection Disposed.");
            }
        }

        private void HandleSynchronizePoll()
        {
            // when it is not a long poll, we need to get the ticks for the audit.
            if (PerformanceAuditor.TicksToHandlePoll == 0)
            {
                GetTicksInHandlingPoll();
            }

            ProduceSasPollData(
                SasPollData.PacketType.Rx,
                SasPollData.PollType.SyncPoll,
                _readData,
                _stopWatch.ElapsedTicks - PerformanceAuditor.TicksToGetPoll);
            Diagnostics.SynchronizePoll++;
        }

        private void ProcessLongPollMessage()
        {
            var bytes = _readData.ToArray();

            if (bytes.Length == 0)
            {
                Logger.Debug("[SAS] Read data has been cleared before processing. Ignoring unidentifiable long poll");
                return;
            }

            Logger.Debug($"[SAS] Got a long poll {BitConverter.ToString(bytes)}");

            var broadcastLongPoll = bytes[0] == 0x00;
            if (IsOtherAddressPoll)
            {
                GetTicksInHandlingPoll();
                return;
            }

            if (IsRealTimeEventReportingActive && !broadcastLongPoll)
            {
                var handleGp = false;
                if (!_messageQueue.IsEmpty)
                {
                    handleGp = true;
                    Logger.Debug("[SAS] Got a long poll with Real Time Events queued. Process pending message instead");
                }
                else if (_exceptionQueue.ConvertRealTimeExceptionToNormal(_exceptionQueue.Peek()) != (byte)GeneralExceptionCode.None)
                {
                    handleGp = true;
                    Logger.Debug("[SAS] Got a long poll with Real Time Events queued. Sending event instead");
                }

                if (handleGp)
                {
                    HandleGeneralPoll();
                    return;
                }
            }

            if (broadcastLongPoll && !LongPollTable.Rows[Command].worksWithGlobalPolls)
            {
                Logger.Debug($"[SAS] this long poll {Command:X2} doesn't support broadcast!");
                return;
            }

            // if this is a LongPoll with crc, validate the crc on the long poll before sending it to the handler
            if (bytes.Length >= LpWithCrcLength && !Utilities.CheckCrcWithSasAddress(bytes))
            {
                if (broadcastLongPoll)
                {
                    Logger.Debug("[SAS] Crc failed. Ignoring global long poll");
                }
                else
                {
                    Logger.Debug("[SAS] Crc failed. NACKing long poll");
                    var res = new List<byte> { (byte)(_port.SasAddress | SasConstants.Nack) };
                    ProduceSasPollData(
                        SasPollData.PacketType.Tx,
                        SasPollData.PollType.LongPoll,
                        res,
                        _stopWatch.ElapsedTicks - PerformanceAuditor.TicksToGetPoll);
                    SendResponse(res);
                }

                Diagnostics.CrcError++;
                GetTicksInHandlingPoll();
                return;
            }

            // Let the Long Poll factory parse the command and create a response if it hasn't been disposed.
            PerformanceAuditor.TicksUsedBeforeProcessingLongPoll = _stopWatch.ElapsedTicks;
            var response = _factory?.GetParserForLongPoll((LongPoll)Command)?.Parse(bytes);

            PerformanceAuditor.TicksToProcessLongPoll = _stopWatch.ElapsedTicks - PerformanceAuditor.TicksUsedBeforeProcessingLongPoll;

            // don't send a response for broadcast long polls
            if (response is null || broadcastLongPoll)
            {
                GetTicksInHandlingPoll();
                if (broadcastLongPoll)
                {
                    Logger.Debug("Handle a broadcast long poll");
                }
                else
                {
                    Diagnostics.UnknownCommand++;
                    Logger.Debug("Ignoring long poll by not responding");
                }
                
                return;
            }

            // if we just get an ACK or NACK response we don't need to add CRC.
            // if we get a non-BUSY response that contains data we do need to add CRC to the response.
            // Note that the BUSY byte indexed with "1" in the response is x00
            if (response.Count > 1 && response.ElementAt(SasConstants.SasResponseBusyIndex) != 0)
            {
                response = Utilities.CalculateAndAddCrc(response.ToArray());
            }

            ProduceSasPollData(
                SasPollData.PacketType.Tx,
                SasPollData.PollType.LongPoll,
                response,
                _stopWatch.ElapsedTicks - PerformanceAuditor.TicksToGetPoll);
            SendResponse(response);

            // We are sending a response which means this message expects an implied ack
            var handlers = _factory?.GetParserForLongPoll((LongPoll)Command)?.Handlers;
            _hostAcknowlegementProvider.SetPendingImpliedAck(new List<byte>(_readData), handlers);
            Logger.Debug($"[SAS] Response to long poll is {BitConverter.ToString(response.ToArray())}");
        }

        private void ResendLastMessage()
        {
            _port.SendRawBytes(_lastMessage);
            Diagnostics.ResendPacket++;
            Diagnostics.ImpliedNack++;
            Logger.Info($"Resending the last message {BitConverter.ToString(_lastMessage.ToArray())}");
        }

        private void UpdateLinkDownStatus()
        {
            if (IsPollAvailable)
            {
                PerformanceAuditor.ChirpWatch.Restart();
                StartLogChirp = true;
                if ((IsGeneralPoll || IsLongPoll) && !LinkUp && _hostAcknowlegementProvider.Synchronized)
                {
                    Logger.Warn("[SAS] the connection to the backend has been recovered.");
                    _callbacks.LinkUp(true, ClientNumber);
                    LinkUp = true;
                    Diagnostics.LastLinkUp = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                    Diagnostics.CommsStatus = CommsStatus.Online;
                }

                return;
            }

            if (PerformanceAuditor.ChirpWatch.ElapsedMilliseconds >= PerformanceAuditor.ChirpTimeout)
            {
                if (PerformanceAuditor.LogChirpWatch.ElapsedMilliseconds >= PerformanceAuditor.LogChirpTimeout || StartLogChirp)
                {
                    StartLogChirp = false;
                    PerformanceAuditor.LogChirpWatch.Restart();
                    Logger.Warn("[SAS] Loop break is detected; sending chirp.");
                    Diagnostics.CommsStatus = CommsStatus.LoopBreak;
                }

                _port.SendChirp();
                Diagnostics.Chirp++;

                if (LinkUp)
                {
                    Logger.Warn("[SAS] the connection to the backend has been broken.");
                    _callbacks.LinkUp(false, ClientNumber);
                    _hostAcknowlegementProvider.LinkDown();
                    LinkUp = false;
                    Diagnostics.LastLinkDown = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                    Diagnostics.CommsStatus = CommsStatus.Offline;  
                }
            }
        }

        // called when we haven't received anything from the comm port in over 5 seconds
        private void HandleLinkDown(object source, EventArgs e)
        {
            Logger.Debug("[SAS] Link down timer expired!");

            // mark link down
            _callbacks.LinkUp(false, ClientNumber);
            LinkUp = false;
            _hostAcknowlegementProvider.LinkDown();
            Diagnostics.LastLinkDown = DateTime.Now.ToString(CultureInfo.CurrentCulture);
        }

        private void PeekPoll()
        {
            _readData.Clear();
            Reset();
            PerformanceAuditor.Reset();
            while (_running &&
                PerformanceAuditor.PollWatch.ElapsedMilliseconds <= PerformanceAuditor.ChirpInterval
                && !IsPollAvailable)
            {
                IsPollAvailable = GetPoll();
            }
        }

        private bool GetPoll()
        {
            (byte @byte, bool wakeupBitSet, bool validData) = _port.ReadOneByte(false);
            var start = validData && wakeupBitSet;
            byte firstByte = @byte;
            byte sasAddress = _port.SasAddress;

            var result = false;
            var continuousWakeUps = 0;
            while (start && continuousWakeUps++ < MaximumContinuousWakeups)
            {
                _readData.Clear();
                Reset();
                PerformanceAuditor.Reset(false);
                _readData.Add(firstByte);
                IsGlobalPoll = (firstByte & SasConstants.SasAddressMask) == 0x00;

                if (firstByte == 0x00)
                {
                    Diagnostics.GlobalBroadcast++;
                }

                IsOtherAddressPoll = !IsGlobalPoll &&
                                       (firstByte & SasConstants.SasAddressMask) != sasAddress;
                if (IsOtherAddressPoll)
                {
                    Diagnostics.OtherAddressedPoll++;
                }
                else
                {
                    Diagnostics.AddressedPoll++;
                }

                IsGeneralPoll = (firstByte == (sasAddress | SasConstants.PollBit));
                if (IsGeneralPoll)
                {
                    ProduceSasPollData(
                        SasPollData.PacketType.Rx,
                        SasPollData.PollType.GeneralPoll,
                        _readData,
                        _stopWatch.ElapsedTicks - PerformanceAuditor.TicksToGetPoll);
                    Diagnostics.GeneralPoll++;
                }

                IsLongPoll = firstByte == sasAddress || @byte == 0;
                if (!IsLongPoll)
                {
                    result = IsGlobalPoll || IsOtherAddressPoll || IsGeneralPoll;
                    break;
                }

                Diagnostics.LongPoll++;
                

                (@byte, wakeupBitSet, validData) = _port.ReadOneByte(true);
                if (PerformanceAuditor.IgnorePackageDueToInterBytesDelay(Logger, firstByte, @byte, true))
                {
                    // Inter-bytes delay exceeds 5ms so this package is treated as invalid.
                    Diagnostics.ErrorInterByteDelay++;
                    break;
                }

                if (!validData)
                {
                    Logger.Warn("[SAS] failed to read the second byte / command for a long poll.");
                    break;
                }

                start = wakeupBitSet;
                if (start)
                {
                    firstByte = @byte;
                    Logger.Info("[SAS] the wakeup bit is set when reading the second byte / command in a long poll.");
                    continue;
                }

                Command = @byte;
                _readData.Add(@byte);
                var len = LongPollTable.Rows[@byte].length;
                if (len == 0)
                {
                    Diagnostics.UnknownCommand++;
                    Logger.Warn($"[SAS] this long poll {_readData[1]:X2} is not supported.");
                    break;
                }

                // the length of type R message is 2 which is the least
                if (len == SasConstants.MinimumBytesForLongPoll)
                {
                    result = true;
                    ProduceSasPollData(
                        SasPollData.PacketType.Rx,
                        SasPollData.PollType.LongPoll,
                        _readData,
                        _stopWatch.ElapsedTicks);
                    break;
                }

                (@byte, wakeupBitSet, validData) = _port.ReadOneByte(true);
                if (PerformanceAuditor.IgnorePackageDueToInterBytesDelay(Logger, _readData[1], @byte))
                {
                    // Inter-bytes delay exceeds 5ms so this package is treated as invalid.
                    Diagnostics.ErrorInterByteDelay++;
                    break;
                }

                if (!validData)
                {
                    Logger.Warn($"[SAS] failed to read the third byte / length for the long poll {_readData[1]:X2}");
                    break;
                }

                start = wakeupBitSet;
                if (start)
                {
                    firstByte = @byte;
                    Logger.Info($"[SAS] got a wakeup bit when reading the third byte / length for the long poll {_readData[1]:X2}");
                    continue;
                }

                _readData.Add(@byte);
                if (LongPollTable.Rows[_readData[1]].isVariableLength)
                {
                    len += @byte;
                }

                var lastByte = @byte;
                while (_readData.Count < len && _running)
                {
                    (@byte, wakeupBitSet, validData) = _port.ReadOneByte(true);
                    if (PerformanceAuditor.IgnorePackageDueToInterBytesDelay(Logger, lastByte, @byte))
                    {
                        // Inter-bytes delay exceeds 5ms so this package is treated as invalid.
                        Diagnostics.ErrorInterByteDelay++;
                        break;
                    }

                    if (!validData)
                    {
                        Logger.Warn($"[SAS] failed to read a byte while reading the long poll {Command:X2}");
                        break;
                    }

                    start = wakeupBitSet;
                    if (start)
                    {
                        firstByte = @byte;
                        Logger.Info($"[SAS] got a wakeup bit while reading bytes for the long poll {Command:X2}");
                        break;
                    }

                    _readData.Add(@byte);
                    lastByte = @byte;
                }

                result = validData && _running;
                ProduceSasPollData(
                    SasPollData.PacketType.Rx,
                    SasPollData.PollType.LongPoll,
                    _readData,
                    _stopWatch.ElapsedTicks);
            }

            PerformanceAuditor.TicksToGetPoll = _stopWatch.ElapsedTicks;
            return result;
        }

        private void Reset()
        {
            IsPollAvailable = false;
            IsGeneralPoll = false;
            IsLongPoll = false;
            IsGlobalPoll = false;
            IsOtherAddressPoll = false;

            _stopWatch.Restart();
        }

        private void GetTicksInHandlingPoll()
        {
            _stopWatch.Stop();
            PerformanceAuditor.TicksToHandlePoll = _stopWatch.ElapsedTicks - PerformanceAuditor.TicksToGetPoll;
        }
    }
}
