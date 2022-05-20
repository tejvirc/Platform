namespace Aristocrat.G2S.Client.Devices.v21
{
    using Newtonsoft.Json;
    using Protocol.v21;

    /// <summary>
    ///     Implement the <see cref="IInformedPlayerDevice" /> device
    /// </summary>
    public class InformedPlayerDevice : ClientDeviceBase<informedPlayer>, IInformedPlayerDevice
    {
        /// <summary>
        ///     The no message timer default value
        /// </summary>
        public const int DefaultNoMessageTimer = 0;

        /// <summary>
        ///     The no host text default value
        /// </summary>
        public const string DefaultNoHostText = @"";

        /// <summary>
        ///     The uncarded money in default value
        /// </summary>
        public const bool DefaultUncardedMoneyIn = true;

        /// <summary>
        ///     The uncarded game play default value
        /// </summary>
        public const bool DefaultUncardedGamePlay = true;

        /// <summary>
        ///     The session start money in default value
        /// </summary>
        public const bool DefaultSessionStartMoneyIn = true;

        /// <summary>
        ///     The session start game play default value
        /// </summary>
        public const bool DefaultSessionStartGamePlay = true;

        /// <summary>
        ///     The session start cash out default value
        /// </summary>
        public const bool DefaultSessionStartCashOut = false;

        /// <summary>
        ///     The session end cash out default value
        /// </summary>
        public const bool DefaultSessionEndCashOut = false;

        /// <summary>
        ///     The session start PIN entry default value
        /// </summary>
        public const bool DefaultSessionStartPinEntry = false;

        /// <summary>
        ///     The session start limit default value
        /// </summary>
        public const int DefaultSessionStartLimit = 0;

        private long _sessionLimit;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InformedPlayerDevice" /> class.
        /// </summary>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="deviceId">The device identifier.</param>
        public InformedPlayerDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver, false)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public bool RestartStatus { get; protected set; }

        /// <inheritdoc />
        [JsonIgnore]
        public IPlayerDevice Player { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public IClass InformedPlayerClassInstance => InternalCreateClass();

        /// <inheritdoc />
        public bool HostActive { get; set; }

        /// <inheritdoc />
        public bool GamePlayEnabled { get; set; }

        /// <inheritdoc />
        public bool MoneyInEnabled { get; set; }

        /// <inheritdoc />
        public int TimeToLive { get; protected set; }

        /// <inheritdoc />
        public int NoMessageTimer { get; set; }

        /// <inheritdoc />
        public string NoHostText { get; set; }

        /// <inheritdoc />
        public bool UnCardedMoneyIn { get; set; }

        /// <inheritdoc />
        public bool UnCardedGamePlay { get; set; }

        /// <inheritdoc />
        public bool SessionStartMoneyIn { get; set; }

        /// <inheritdoc />
        public bool SessionStartGamePlay { get; set; }

        /// <inheritdoc />
        public bool SessionStartCashOut { get; set; }

        /// <inheritdoc />
        public bool SessionEndCashOut { get; set; }

        /// <inheritdoc />
        public bool SessionStartPinEntry { get; set; }

        /// <inheritdoc />
        public long SessionLimit
        {
            get => _sessionLimit;
            set
            {
                var isNew = _sessionLimit != value;
                _sessionLimit = value;
                if (isNew)
                {
                    EventReport(EventCode.G2S_IPE104);
                }
            }
        }

        /// <inheritdoc />
        public long SessionStartLimit { get; set; }

        /// <inheritdoc />
        public override void Close()
        {
        }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
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
                G2SParametersNames.InformedPlayerDevice.NoMessageTimerParameterName,
                optionConfigValues,
                parameterId => { NoMessageTimer = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.InformedPlayerDevice.NoHostTextParameterName,
                optionConfigValues,
                parameterId => { NoHostText = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.InformedPlayerDevice.UncardedMoneyInParameterName,
                optionConfigValues,
                parameterId => { UnCardedMoneyIn = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.InformedPlayerDevice.UncardedGamePlayParameterName,
                optionConfigValues,
                parameterId => { UnCardedGamePlay = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.InformedPlayerDevice.SessionStartMoneyInParameterName,
                optionConfigValues,
                parameterId => { SessionStartMoneyIn = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.InformedPlayerDevice.SessionStartGamePlayParameterName,
                optionConfigValues,
                parameterId => { SessionStartGamePlay = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.InformedPlayerDevice.SessionStartCashOutParameterName,
                optionConfigValues,
                parameterId => { SessionStartCashOut = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.InformedPlayerDevice.SessionEndCashOutParameterName,
                optionConfigValues,
                parameterId => { SessionEndCashOut = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.InformedPlayerDevice.SessionStartPinEntryParameterName,
                optionConfigValues,
                parameterId => { SessionStartPinEntry = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.InformedPlayerDevice.SessionStartLimitParameterName,
                optionConfigValues,
                parameterId => { SessionStartLimit = optionConfigValues.Int32Value(parameterId); });

            EventReport(EventCode.G2S_IPE005);
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            //EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE001); // not used
            //EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE002); // not used
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE005);
            //EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE006);  // not used

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE100);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE104);
            //EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE107); // not used
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE108); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE109); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE110); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE111); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE112);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE113); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_IPE114);
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        private void EventReport(string eventStr, bool noStatus = false)
        {
            var status = new ipStatus
            {
                configurationId = ConfigurationId,
                configDateTime = ConfigDateTime,
                configComplete = ConfigComplete,
                egmEnabled = Enabled,
                hostEnabled = HostEnabled,
                hostActive = HostActive,
                moneyInEnabled = MoneyInEnabled,
                gamePlayEnabled = GamePlayEnabled,
                sessionLimit = SessionLimit
            };

            var deviceList = this.DeviceList(status);

            EventHandlerDevice.EventReport(
                this.PrefixedDeviceClass(),
                Id,
                eventStr,
                noStatus ? null : deviceList);
        }

        private void SetDefaults()
        {
            Player = null;
            RestartStatus = true;
            TimeToLive = (int)Constants.DefaultTimeout.TotalMilliseconds;
            HostActive = false;
            NoMessageTimer = DefaultNoMessageTimer;
            NoHostText = DefaultNoHostText;
            UnCardedMoneyIn = DefaultUncardedMoneyIn;
            UnCardedGamePlay = DefaultUncardedGamePlay;
            SessionStartMoneyIn = DefaultSessionStartMoneyIn;
            SessionStartGamePlay = DefaultSessionStartGamePlay;
            SessionStartCashOut = DefaultSessionStartCashOut;
            SessionEndCashOut = DefaultSessionEndCashOut;
            SessionStartPinEntry = DefaultSessionStartPinEntry;
            SessionStartLimit = DefaultSessionStartLimit;
        }
    }
}
