namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     The printer class is used to manage thermal printing devices installed in an EGM. The class is designed to be used
    ///     with printers that support the GDS Printer protocol published by the Gaming Standards Association.
    /// </summary>
    public class PrinterDevice : ClientDeviceBase<printer>, IPrinterDevice
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterDevice" /> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public PrinterDevice(int deviceId, IDeviceObserver deviceObserver)
            : base(deviceId, deviceObserver)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public bool RestartStatus { get; protected set; }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void Close()
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
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE006);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE099);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE103);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE201);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE202);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE203); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE204); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE205);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE206);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE207);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE208); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE209);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE901);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE902);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE903); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE904);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE905);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE906); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE907);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PTE908); //// TODO
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
            RequiredForPlay = true;
        }
    }
}