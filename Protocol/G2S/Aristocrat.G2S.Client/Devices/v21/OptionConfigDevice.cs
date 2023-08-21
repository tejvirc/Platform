namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using Protocol.v21;

    /// <summary>
    ///     The optionConfig class is used to configure options available within an EGM. This class includes commands
    ///     to change device-specific options excluding device ownerships.There is no requirement that an EGM support
    ///     the optionConfig class. Alternatively, options covered by the optionConfig class can be set manually through
    ///     the operator mode of the EGM; however, the optionConfig class provides a convenient and efficient
    ///     mechanism to remotely configure EGMs.
    /// </summary>
    public class OptionConfigDevice : HostOrientedDevice<optionConfig>, IOptionConfigDevice
    {
        /// <summary>
        ///     The default no response timer.
        /// </summary>
        public const int DefaultNoResponseTimer = 300000;

        private const int DefaultMinLogEntries = 35;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigDevice" /> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public OptionConfigDevice(int deviceId, IDeviceObserver deviceObserver)
            : base(deviceId, deviceObserver)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public int MinLogEntries { get; protected set; }

        /// <inheritdoc />
        public TimeSpan NoResponseTimer { get; protected set; }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void Close()
        {
        }

        /// <inheritdoc />
        public void OptionChangeStatus(optionChangeStatus changeStatus)
        {
            var request = InternalCreateClass();
            request.Item = changeStatus;

            var session = SendRequest(request);
            session.WaitForCompletion();
        }

        /// <inheritdoc />
        public void OptionsChanged(deviceOptions[] options)
        {
            var request = InternalCreateClass();
            request.Item = new optionList { deviceOptions = options };

            var session = SendRequest(request);
            session.WaitForCompletion();
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            base.ApplyOptions(optionConfigValues);

            SetDeviceValue(
                G2SParametersNames.NoResponseTimerParameterName,
                optionConfigValues,
                parameterId => { NoResponseTimer = optionConfigValues.TimeSpanValue(parameterId); });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE007);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE008);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE107);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE108);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE109);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_OCE110);
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        private void SetDefaults()
        {
            MinLogEntries = DefaultMinLogEntries;
            NoResponseTimer = TimeSpan.FromMilliseconds(DefaultNoResponseTimer);
        }
    }
}