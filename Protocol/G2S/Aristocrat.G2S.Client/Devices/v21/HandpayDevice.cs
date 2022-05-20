namespace Aristocrat.G2S.Client.Devices.v21
{
    using Diagnostics;
    using Protocol.v21;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    ///     The <see cref="handpay" /> class is used to manage large wins (i.e wins greater than cabinetProfile.largeWinLimit) and
    ///     other payouts that cannot be made by the EGM, including cancelled-credit handpays, external bonus
    ///     handpays, and DFT handpays.
    /// </summary>
    public class HandpayDevice : ClientDeviceBase<handpay>, IHandpayDevice
    {
        private volatile bool _open;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayDevice" /> class.
        /// </summary>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="deviceId">The device identifier.</param>
        public HandpayDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public bool RestartStatus { get; private set; }

        /// <inheritdoc />
        public int TimeToLive { get; private set; }

        /// <inheritdoc />
        public int IdReaderId { get; protected set; }

        /// <inheritdoc />
        public int MinLogEntries { get; protected set; }

        /// <inheritdoc />
        public async Task<bool> Request(handpayRequest command)
        {
            if (!_open || !Queue.CanSend)
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"HandpayDevice.Request : request cannot send or device is closed
    Device Id : {0}
    Transaction Id : {1}",
                    Id,
                    command.transactionId);

                return false;
            }

            var request = InternalCreateClass();
            request.Item = command;

            while (_open && Queue.CanSend)
            {
                var session = SendRequest(request, TimeSpan.FromMilliseconds(TimeToLive));

                var timeout = TimeSpan.FromMilliseconds((TimeToLive < 30000 ? 30000 : TimeToLive) * 2);

                session.WaitForCompletion(timeout);

                if (session.SessionState == SessionStatus.CommsLost)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"HandpayDevice.Request : request Comms Lost Host Id : {0}",
                        Owner);

                    break;
                }

                if (session.SessionState == SessionStatus.TimedOut)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"HandpayDevice.Request : request Timed Out Host Id : {0}",
                        Owner);

                    continue;
                }

                if (session.SessionState != SessionStatus.Success)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"HandpayDevice.Request : request Failed Host Id : {0}",
                        Owner);

                    await Task.Delay(500);

                    continue;
                }

                var response = session.Responses.FirstOrDefault();

                if (!(response?.IClass.Item is handpayAck ack) || ack.transactionId != command.transactionId)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"HandpayDevice.Request : request Invalid Response Host Id : {0}",
                        Owner);

                    continue;
                }

                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"HandpayDevice.Request : request successful Host Id : {0}",
                    Owner);

                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        /// <inheritdoc />
        public async Task<bool> KeyedOff(keyedOff command)
        {
            if (!_open || !Queue.CanSend)
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"HandpayDevice.KeyedOff : keyed off cannot send or device is closed
    Device Id : {0}
    Transaction Id : {1}",
                    Id,
                    command.transactionId);

                return false;
            }

            var request = InternalCreateClass();
            request.Item = command;

            while (_open && Queue.CanSend)
            {
                var session = SendRequest(request, TimeSpan.FromMilliseconds(TimeToLive));

                var timeout = TimeSpan.FromMilliseconds((TimeToLive < 30000 ? 30000 : TimeToLive) * 2);

                session.WaitForCompletion(timeout);

                if (session.SessionState == SessionStatus.CommsLost)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"HandpayDevice.KeyedOff : keyed off Comms Lost Host Id : {0}",
                        Owner);

                    break;
                }

                if (session.SessionState == SessionStatus.TimedOut)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"HandpayDevice.KeyedOff : keyed off Timed Out Host Id : {0}",
                        Owner);

                    continue;
                }

                if (session.SessionState != SessionStatus.Success)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"HandpayDevice.KeyedOff : keyed off Failed Host Id : {0}",
                        Owner);

                    continue;
                }

                var response = session.Responses.FirstOrDefault();

                if (!(response?.IClass.Item is keyedOffAck ack) || ack.transactionId != command.transactionId)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"HandpayDevice.KeyedOff : keyed off Invalid Response Host Id : {0}",
                        Owner);

                    continue;
                }

                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"HandpayDevice.KeyedOff : keyed off successful Host Id : {0}",
                    Owner);

                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
            _open = true;
        }

        /// <inheritdoc />
        public override void Close()
        {
            _open = false;
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
                G2SParametersNames.GatDevice.IdReaderIdParameterName,
                optionConfigValues,
                parameterId => { IdReaderId = optionConfigValues.Int32Value(parameterId); });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE006);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_JPE106);
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        private void SetDefaults()
        {
            RestartStatus = true;
            MinLogEntries = Constants.DefaultMinLogEntries;
        }
    }
}
