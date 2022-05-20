namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Diagnostics;
    using Protocol.v21;

    /// <summary>
    ///     Defines a G2S SPC client device.
    /// </summary>
    public class SpcDevice : ClientDeviceBase<spc>, ISpcDevice
    {
        private volatile bool _open;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpcDevice" /> class.
        /// </summary>
        /// <param name="deviceId">device id</param>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public SpcDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public bool RestartStatus { get; set; }

        /// <inheritdoc />
        public int TimeToLive { get; set; }

        /// <inheritdoc />
        public int LevelId { get; set; }

        /// <inheritdoc />
        public string ControllerType { get; set; }

        /// <inheritdoc />
        public int ResetAmount { get; set; }

        /// <inheritdoc />
        public int MaxLevelAmount { get; set; }

        /// <inheritdoc />
        public int ContribPercent { get; set; }

        /// <inheritdoc />
        public bool RoundingEnabled { get; set; }

        /// <inheritdoc />
        public int MysteryMinimum { get; set; }

        /// <inheritdoc />
        public int MysteryMaximum { get; set; }

        /// <inheritdoc />
        public int GamePlayId { get; set; }

        /// <inheritdoc />
        public int WinLevelIndex { get; set; }

        /// <inheritdoc />
        public string PaytableId { get; set; }

        /// <inheritdoc />
        public string ThemeId { get; set; }

        /// <inheritdoc />
        public int DenomId { get; set; }

        /// <inheritdoc />
        public int MinLogEntries { get; set; }

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
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_SPE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_SPE002);
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
        public async Task<bool> LevelReset(spcLevelReset command)
        {
            if (!(_open && Queue.CanSend))
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"SpcDevice.LevelReset : cannot send or device is closed \
                            Device Id : {0} Transaction Id : {1}",
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
                        @"SpcDevice.LevelReset : Comms Lost Host Id : {0}",
                        Owner);

                    break;
                }

                if (session.SessionState == SessionStatus.TimedOut)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"SpcDevice.LevelReset : Timed Out Host Id : {0}",
                        Owner);

                    continue;
                }

                if (session.SessionState != SessionStatus.Success)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"SpcDevice.LevelReset : Failed Host Id : {0}",
                        Owner);

                    continue;
                }

                var response = session.Responses.FirstOrDefault();

                if (!(response?.IClass.Item is keyedOffAck ack) || ack.transactionId != command.transactionId)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"SpcDevice.LevelReset : Invalid Response Host Id : {0}",
                        Owner);

                    continue;
                }

                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"SpcDevice.LevelReset : successful Host Id : {0}",
                    Owner);

                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
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
            RequiredForPlay = false;
            LevelId = 2;
            ControllerType = "G2S_standaloneProg";
            ResetAmount = 100000000;
            MaxLevelAmount = 1000000000;
            ContribPercent = 15000;
            RoundingEnabled = true;
            MysteryMinimum = 500000000;
            MysteryMaximum = 1000000000;
            GamePlayId = 0;
            WinLevelIndex = 0;
            PaytableId = string.Empty;
            ThemeId = string.Empty;
            DenomId = 1000;
            MinLogEntries = Constants.DefaultMinLogEntries;
        }
    }
}
