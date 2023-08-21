namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Threading.Tasks;
    using Protocol.v21;

    /// <summary>
    ///     The central class is used to manage the distribution of finite chances between a central determinant game host and
    ///     an EGM.The central class includes commands and events to request and issue finite chances and to associate EGM game
    ///     combos and wager categories with finite deals supported by the central host.The central class also includes
    ///     commands to retrieve the central transaction log maintained by the EGM.
    /// </summary>
    public class CentralDevice : ClientDeviceBase<central>, ICentralDevice
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusDevice" /> class.
        /// </summary>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public CentralDevice(IDeviceObserver deviceStateObserver)
            : base(1, deviceStateObserver)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void Close()
        {
        }

        /// <inheritdoc />
        public bool RestartStatus { get; private set; }

        /// <inheritdoc />
        public int TimeToLive { get; private set; }

        /// <inheritdoc />
        public TimeSpan NoResponseTimer { get; private set; }

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
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CLE001); // not used
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CLE002); // not used
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CLE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CLE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CLE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CLE006); // not used

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CLE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CLE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CLE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CLE104);
        }

        /// <inheritdoc />
        public Task<centralOutcome> GetOutcome(getCentralOutcome outcome)
        {
            var request = InternalCreateClass();
            request.Item = outcome;

            var session = SendRequest(request);
            session.WaitForCompletion(); // wait for the session to complete

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0)
            {
                if (session.Responses[0].IClass.Item is centralOutcome ack)
                {
                    return Task.FromResult(ack);
                }
            }

            return Task.FromResult((centralOutcome)null);
        }

        /// <inheritdoc />
        public Task<bool> CommitOutcome(commitOutcome outcome)
        {
            var request = InternalCreateClass();
            request.Item = outcome;

            var session = SendRequest(request);
            session.WaitForCompletion(); // wait for the session to complete

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0)
            {
                if (session.Responses[0].IClass.Item is commitOutcomeAck ack)
                {
                    return Task.FromResult(ack.transactionId == outcome.transactionId);
                }
            }

            return Task.FromResult(false);
        }

        private void SetDefaults()
        {
            RestartStatus = true;
            RequiredForPlay = false;
            NoResponseTimer = Constants.DefaultTimeout;
        }
    }
}