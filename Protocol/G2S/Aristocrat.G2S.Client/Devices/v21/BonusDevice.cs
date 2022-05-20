namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Protocol.v21;

    /// <summary>
    ///     The bonus class is used to manage the process of awarding external bonuses at an EGM. External bonuses encompass a
    ///     wide variety of game enhancements for the player, such as win multipliers, mystery progressives, and consolation
    ///     prizes.External bonus awards are paid in addition to the standard paytable awards.
    /// </summary>
    public class BonusDevice : ClientDeviceBase<bonus>, IBonusDevice
    {
        private readonly object _timerLock = new object();

        private Timer _commsTimer;

        private Action<bool> _onStateChange;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusDevice" /> class.
        /// </summary>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="deviceId">The device identifier.</param>
        public BonusDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver, false)
        {
            SetDefaults();

            _commsTimer = new Timer(
                _ => SetHostActive(false),
                null,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);
        }

        /// <inheritdoc />
        public int TimeToLive { get; private set; }

        /// <inheritdoc />
        public bool RestartStatus { get; private set; }

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
                G2SParametersNames.TimeToLiveParameterName,
                optionConfigValues,
                parameterId => { TimeToLive = optionConfigValues.Int32Value(parameterId); });

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
                G2SParametersNames.BonusDevice.NoMessageTimerParameterName,
                optionConfigValues,
                parameterId => { NoResponseTimer = TimeSpan.FromMilliseconds(optionConfigValues.Int32Value(parameterId)); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.NoHostTextParameterName,
                optionConfigValues,
                parameterId => { NoHostText = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.IdReaderIdParameterName,
                optionConfigValues,
                parameterId => { IdReaderId = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.UsePlayerIdReaderParameterName,
                optionConfigValues,
                parameterId => { UsePlayerIdReader = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.BonusEligibilityTimer,
                optionConfigValues,
                parameterId => { EligibilityTimer = TimeSpan.FromMilliseconds(optionConfigValues.Int32Value(parameterId)); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.MaximumBonusLimit,
                optionConfigValues,
                parameterId => { DisplayLimit = optionConfigValues.Int64Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.MaximumBonusLimitText,
                optionConfigValues,
                parameterId => { DisplayLimitText = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.DisplayLimitDuration,
                optionConfigValues,
                parameterId => { DisplayLimitDuration = TimeSpan.FromMilliseconds(optionConfigValues.Int32Value(parameterId)); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.WagerMatchCardRequired,
                optionConfigValues,
                parameterId => { WagerMatchCardRequired = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.WagerMatchLimit,
                optionConfigValues,
                parameterId => { WagerMatchLimit = optionConfigValues.Int64Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.WagerMatchLimitText,
                optionConfigValues,
                parameterId => { WagerMatchLimitText = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.WagerMatchLimitDuration,
                optionConfigValues,
                parameterId => { WagerMatchLimitDuration = TimeSpan.FromMilliseconds(optionConfigValues.Int32Value(parameterId)); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.WagerMatchExitText,
                optionConfigValues,
                parameterId => { WagerMatchExitText = optionConfigValues.StringValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.BonusDevice.WagerMatchExitDuration,
                optionConfigValues,
                parameterId => { WagerMatchExitDuration = TimeSpan.FromMilliseconds(optionConfigValues.Int32Value(parameterId)); });

            UpdateCommsTimer();
        }

        /// <inheritdoc />
        public bool HostActive { get; private set; }

        /// <inheritdoc />
        public bool BonusActive { get; private set; }

        /// <inheritdoc />
        public string NoHostText { get; private set; }

        /// <inheritdoc />
        public bool UsePlayerIdReader { get; private set; }

        /// <inheritdoc />
        public TimeSpan EligibilityTimer { get; private set; }

        /// <inheritdoc />
        public long DisplayLimit { get; private set; }

        /// <inheritdoc />
        public string DisplayLimitText { get; private set; }

        /// <inheritdoc />
        public TimeSpan DisplayLimitDuration { get; private set; }

        /// <inheritdoc />
        public bool WagerMatchCardRequired { get; private set; }

        /// <inheritdoc />
        public long WagerMatchLimit { get; private set; }

        /// <inheritdoc />
        public string WagerMatchLimitText { get; private set; }

        /// <inheritdoc />
        public TimeSpan WagerMatchLimitDuration { get; private set; }

        /// <inheritdoc />
        public string WagerMatchExitText { get; private set; }

        /// <inheritdoc />
        public TimeSpan WagerMatchExitDuration { get; private set; }

        /// <inheritdoc />
        public TimeSpan NoResponseTimer { get; private set; }

        /// <inheritdoc />
        public int IdReaderId { get; private set; }

        /// <inheritdoc />
        public void NotifyActive()
        {
            lock (_timerLock)
            {
                UpdateCommsTimer();

                SetHostActive(true);
            }
        }

        /// <inheritdoc />
        public void SetKeepAlive(bool enabled, Action<bool> onStateChange)
        {
            BonusActive = enabled;

            lock (_timerLock)
            {
                _onStateChange = onStateChange;

                UpdateCommsTimer();

                SetHostActive(true);
            }
        }

        /// <inheritdoc />
        public Task<bool> CommitBonus(commitBonus commitBonus)
        {
            var request = InternalCreateClass();
            request.Item = commitBonus;

            var session = SendRequest(request);
            session.WaitForCompletion(); // wait for the session to complete

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0)
            {
                if (session.Responses[0].IClass.Item is commitBonusAck ack)
                {
                    return Task.FromResult(ack.transactionId == commitBonus.transactionId);
                }
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE006);  // Not used
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE007);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE008);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE107);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE108);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE109);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE110);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE111);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_BNE201);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_BNE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_BNE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_BNE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_BNE005);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_timerLock)
                {
                    if (_commsTimer != null)
                    {
                        _commsTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                        _commsTimer.Dispose();
                    }
                }
            }

            _commsTimer = null;

            base.Dispose(disposing);
        }

        private void SetDefaults()
        {
            RestartStatus = true;
            RequiredForPlay = false;
            NoResponseTimer = Constants.DefaultTimeout;
            IdReaderId = 0;
            UsePlayerIdReader = false;
            EligibilityTimer = G2SParametersNames.BonusDevice.EligibilityTimerDefault;
            DisplayLimitDuration = G2SParametersNames.BonusDevice.DisplayLimitDurationDefault;
            DisplayLimit = G2SParametersNames.BonusDevice.LimitDefault;
            WagerMatchLimitDuration = G2SParametersNames.BonusDevice.WagerMatchLimitDurationDefault;
            WagerMatchExitDuration = G2SParametersNames.BonusDevice.WagerMatchExitDurationDefault;
            WagerMatchLimit = G2SParametersNames.BonusDevice.LimitDefault;
        }

        private void SetHostActive(bool enabled)
        {
            if (HostActive != enabled)
            {
                HostActive = enabled;

                _onStateChange?.Invoke(enabled);
            }
        }

        private void UpdateCommsTimer()
        {
            if (BonusActive && NoResponseTimer != TimeSpan.Zero)
            {
                _commsTimer.Change(NoResponseTimer, Timeout.InfiniteTimeSpan);
            }
            else
            {
                _commsTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
        }
    }
}