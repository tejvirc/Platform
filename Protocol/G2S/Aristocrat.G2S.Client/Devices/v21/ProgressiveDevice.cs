namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Diagnostics;
    using Protocol.v21;

    /// <summary>
    ///     create class ProgressiveDevice
    /// </summary>
    public class ProgressiveDevice : ClientDeviceBase<progressive>, IProgressiveDevice
    {
        private const int EgmTimeoutException = 1;
        private const int HostRejectedException = 2;

        private bool _deviceCommunicationClosed;
        private bool _disposed;

        private CancellationTokenSource _getProgressiveHostInfoCancellationToken;
        private CancellationTokenSource _progressiveCommitCancellationToken;
        private CancellationTokenSource _progressiveHitCancellationToken;

        /// <summary>
        ///     ProgressiveDevice constructor
        /// </summary>
        /// <param name="deviceId">that is device id of progressive</param>
        /// <param name="deviceStateObserver">deviceStateObserver instance</param>
        public ProgressiveDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver, false)
        {
            SetDefaults();
        }

        /// <summary>
        ///     ProgressiveId
        /// </summary>
        public int ProgressiveId { get; set; }

        /// <summary>
        ///     NoProgressiveInfo
        /// </summary>
        public int NoProgressiveInfo { get; set; }

        /// <summary>
        ///     TimeToLive
        /// </summary>
        public int TimeToLive { get; set; }

        /// <summary>
        ///     RestartStatus
        /// </summary>
        public bool RestartStatus { get; set; }

        /// <summary>
        ///     NoResponseTimer
        /// </summary>
        public TimeSpan NoResponseTimer { get; set; }

        /// <inheritdoc />
        public override void Close()
        {
            _deviceCommunicationClosed = true;
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            base.ApplyOptions(optionConfigValues);

            SetDeviceValue(
                G2SParametersNames.RestartStatusParameterName,
                optionConfigValues,
                parameterId => { RestartStatus = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.UseDefaultConfigParameterName,
                optionConfigValues,
                parameterId => { UseDefaultConfig = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.RequiredForPlayParameterName,
                optionConfigValues,
                parameterId => { RequiredForPlay = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.TimeToLiveParameterName,
                optionConfigValues,
                parameterId => { TimeToLive = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.NoResponseTimerParameterName,
                optionConfigValues,
                parameterId => { NoResponseTimer = optionConfigValues.TimeSpanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.ProgressiveDevice.NoProgInfoParameterName,
                optionConfigValues,
                parameterId => { NoProgressiveInfo = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.ProgressiveDevice.ProgIdParameterName,
                optionConfigValues,
                parameterId => { ProgressiveId = optionConfigValues.Int32Value(parameterId); });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE006);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE009);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE010);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PGE106);
        }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
            _deviceCommunicationClosed = false;
        }

        /// <inheritdoc />
        public progressiveHostInfo GetProgressiveHostInfo(getProgressiveHostInfo command, TimeSpan timeout)
        {
            if (!HostEnabled
                || !Queue.CanSend && !_deviceCommunicationClosed
                || _getProgressiveHostInfoCancellationToken != null && _getProgressiveHostInfoCancellationToken.IsCancellationRequested)
            {
                _getProgressiveHostInfoCancellationToken?.Cancel(false);
                _getProgressiveHostInfoCancellationToken?.Dispose();
                _getProgressiveHostInfoCancellationToken = null;
                return null;
            }

            // Check to see if this request to G2S Host has expired.
            if (timeout < TimeSpan.Zero)
            {
                return null;
            }

            // Update the expiry time to complete.
            if (timeout != TimeSpan.MaxValue)
            {
                timeout = timeout.Subtract(TimeSpan.FromMilliseconds(TimeToLive));
            }

            var request = InternalCreateClass();
            request.Item = command;
            var session = SendRequest(request);
            session.WaitForCompletion();

            var response = session.Responses.FirstOrDefault();

            // Process a successful and adequate response.
            if (session.SessionState == SessionStatus.Success && response != null)
            {
                _getProgressiveHostInfoCancellationToken?.Cancel(false);
                _getProgressiveHostInfoCancellationToken?.Dispose();
                _getProgressiveHostInfoCancellationToken = null;

                return response.IClass.Item as progressiveHostInfo;
            }

            if (_getProgressiveHostInfoCancellationToken == null)
            {
                _getProgressiveHostInfoCancellationToken = new CancellationTokenSource();
            }

            if (session.SessionState == SessionStatus.TimedOut)
            {
                SourceTrace.TraceInformation(G2STrace.Source, @"ProgressiveDevice.GetProgressiveHostInfo : c_baseCommand Timed Out.  Will try again
                                             Device Id : {0}", Id);

                return Task.Run(
                        () => GetProgressiveHostInfo(command, timeout),
                        _getProgressiveHostInfoCancellationToken.Token)
                    .Result;
            }

            SourceTrace.TraceInformation(G2STrace.Source, @"ProgressiveDevice.GetProgressiveHostInfo : c_baseCommand Failed.  Will try again
                                         Device Id : {0}", Id);

            progressiveHostInfo result = null;
            Task.Delay(TimeToLive, _getProgressiveHostInfoCancellationToken.Token)
                .ContinueWith(
                    task =>
                    {
                        if (!task.IsCanceled)
                        {
                            result = GetProgressiveHostInfo(command, timeout);
                        }
                    });

            return result;
        }

        /// <inheritdoc />
        public async Task<progressiveCommitAck> ProgressiveCommit(
            progressiveCommit command,
            progressiveLog progressiveLog,
            t_progStates progressiveStates)
        {
            if (!HostEnabled
                || !Queue.CanSend && !_deviceCommunicationClosed
                || progressiveStates != t_progStates.G2S_progCommit
                || progressiveStates != t_progStates.G2S_progFailed
                || _progressiveCommitCancellationToken != null && _progressiveCommitCancellationToken.IsCancellationRequested)
            {
                _progressiveCommitCancellationToken?.Cancel(false);
                _progressiveCommitCancellationToken?.Dispose();
                _progressiveCommitCancellationToken = null;
                return null;
            }

            var request = InternalCreateClass();
            request.Item = command;
            var session = SendRequest(request);
            session.WaitForCompletion();

            var response = session.Responses.FirstOrDefault();

            if (session.SessionState == SessionStatus.Success && response != null)
            {
                _progressiveCommitCancellationToken?.Cancel(false);
                _progressiveCommitCancellationToken?.Dispose();
                _progressiveCommitCancellationToken = null;
                return response.IClass.Item as progressiveCommitAck;
            }

            if (_progressiveCommitCancellationToken == null)
            {
                _progressiveCommitCancellationToken = new CancellationTokenSource();
            }

            if (session.SessionState == SessionStatus.TimedOut)
            {
                SourceTrace.TraceInformation(G2STrace.Source, @"ProgressiveDevice.progressiveCommit : c_baseCommand Timed Out.  Will try again
                                             Device Id : {0}", Id);

                return await Task.Run(
                    async () => await ProgressiveCommit(command, progressiveLog, progressiveStates),
                    _progressiveCommitCancellationToken.Token);
            }

            SourceTrace.TraceInformation(G2STrace.Source, @"ProgressiveDevice.progressiveCommit : c_baseCommand Failed.  Will try again
                                         Device Id : {0}", Id);

            progressiveCommitAck result = null;
            await Task.Delay(TimeToLive, _progressiveCommitCancellationToken.Token)
                .ContinueWith(
                    async task =>
                    {
                        if (!task.IsCanceled)
                        {
                            result = await ProgressiveCommit(command, progressiveLog, progressiveStates);
                        }
                    });

            return result;
        }

        /// <inheritdoc />
        public async Task<setProgressiveWin> ProgressiveHit(
            progressiveHit command,
            t_progStates progressiveStates,
            progressiveLog progressiveLog,
            TimeSpan timeout)
        {
            if (!HostEnabled
                || !Queue.CanSend && !_deviceCommunicationClosed
                || progressiveStates != t_progStates.G2S_progHit
                || _progressiveHitCancellationToken != null && _progressiveHitCancellationToken.IsCancellationRequested)
            {
                _progressiveCommitCancellationToken?.Cancel(false);
                _progressiveCommitCancellationToken?.Dispose();
                _progressiveCommitCancellationToken = null;
                return null;
            }

            if (timeout < TimeSpan.Zero)
            {
                return null;
            }

            // Update the expiry time to complete.
            if (timeout != TimeSpan.MaxValue)
            {
                timeout = timeout.Subtract(TimeSpan.FromMilliseconds(TimeToLive));
            }

            setProgressiveWin result = null;

            var request = InternalCreateClass();
            request.Item = command;

            var session = SendRequest(request);
            session.WaitForCompletion();

            var response = session.Responses.FirstOrDefault();

            if (session.SessionState == SessionStatus.Success && response != null)
            {
                _progressiveHitCancellationToken?.Cancel(false);
                _progressiveHitCancellationToken?.Dispose();
                _progressiveHitCancellationToken = null;

                return response.IClass.Item as setProgressiveWin;
            }

            if (session.SessionState == SessionStatus.ResponseError && response != null
                && response.IClass.errorCode == ErrorCode.G2S_PGX006)
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"ProgressiveDevice.ProgressiveHit : ProgressiveHit Rejected by host for Device Id : {0}",
                    Id);

                await OnProgressiveHitFailure(command, HostRejectedException, progressiveLog);
                return null;
            }

            if (_progressiveHitCancellationToken == null)
            {
                _progressiveHitCancellationToken = new CancellationTokenSource();
            }

            if (session.SessionState == SessionStatus.TimedOut)
            {
                SourceTrace.TraceInformation(G2STrace.Source, @"ProgressiveDevice.ProgressiveHit : c_baseCommand Timed Out.  Will try again
                                             Device Id : {0}", Id);

                await OnProgressiveHitFailure(command, EgmTimeoutException, progressiveLog);

                return await Task.Run(
                    async () => await ProgressiveHit(command, progressiveStates, progressiveLog, timeout),
                    _progressiveHitCancellationToken.Token);
            }

            SourceTrace.TraceInformation(G2STrace.Source, @"ProgressiveDevice.ProgressiveHit : c_baseCommand Failed.  Will try again
                                         Device Id : {0}", Id);

            await Task.Delay(TimeToLive, _progressiveHitCancellationToken.Token)
                .ContinueWith(
                    async task =>
                    {
                        if (!task.IsCanceled)
                        {
                            result = await ProgressiveHit(command, progressiveStates, progressiveLog, timeout);
                        }
                    });

            return result;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Dispose to free resources
        /// </summary>
        /// <param name="disposing">bool</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed)
            {
                return;
            }

            if (_getProgressiveHostInfoCancellationToken != null)
            {
                _getProgressiveHostInfoCancellationToken.Cancel(false);
                _getProgressiveHostInfoCancellationToken.Dispose();
                _getProgressiveHostInfoCancellationToken = null;
            }

            if (_progressiveCommitCancellationToken != null)
            {
                _progressiveCommitCancellationToken.Cancel(false);
                _progressiveCommitCancellationToken.Dispose();
                _progressiveCommitCancellationToken = null;
            }

            if (_progressiveHitCancellationToken != null)
            {
                _progressiveHitCancellationToken.Cancel(false);
                _progressiveHitCancellationToken.Dispose();
                _progressiveHitCancellationToken = null;
            }

            _disposed = true;
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        /// <summary>
        ///     Handles progressive hit failure
        /// </summary>
        /// <param name="command">progressiveHit command</param>
        /// <param name="progException">progressive exception number</param>
        /// <param name="progressiveLog">progressiveLog data</param>
        private async Task OnProgressiveHitFailure(
            progressiveHit command,
            int progException,
            progressiveLog progressiveLog)
        {
            progressiveLog.progPaidAmt = 0;
            progressiveLog.progState = t_progStates.G2S_progFailed;
            progressiveLog.progException = progException;
            progressiveLog.payMethod = t_progPayMethods.G2S_payHandpay;

            // Send Event to host
            EventHandlerDevice.EventReport(
                this.PrefixedDeviceClass(),
                Id,
                EventCode.G2S_PGE106,
                transactionId: command.transactionId,
                transactionList: new transactionList
                {
                    transactionInfo = new[]
                    {
                        new transactionInfo
                        {
                            deviceId = Id,
                            deviceClass = this.PrefixedDeviceClass(),
                            Item = progressiveLog
                        }
                    }
                });

            // calling Progressive commit handler when Progressive exception occurs 1 or 2
            await ProgressiveCommit(
                new progressiveCommit
                {
                    progException = progException,
                    progPaidAmt = 0,
                    progWinAmt = 0,
                    progWinSeq = 0,
                    progWinText = string.Empty,
                    levelId = command.levelId,
                    progId = command.progId,
                    transactionId = command.transactionId,
                    paidDateTime = DateTime.UtcNow,
                    payMethod = t_progPayMethods.G2S_payHandpay
                },
                progressiveLog,
                progressiveLog.progState);
        }

        private void SetDefaults()
        {
            RequiredForPlay = true;
            RestartStatus = true;
            TimeToLive = (int)Constants.DefaultTimeout.TotalMilliseconds;
            NoResponseTimer = Constants.NoResponseTimer;
        }
    }
}