namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    /// </summary>
    public class ChooserDevice : ClientDeviceBase<chooser>, IChooserDevice
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ChooserDevice" /> class.
        /// </summary>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="deviceId">The device identifier.</param>
        public ChooserDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver, false)
        {
        }

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
                G2SParametersNames.ConfigurationIdParameterName,
                optionConfigValues,
                parameterId => { ConfigurationId = optionConfigValues.Int64Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.UseDefaultConfigParameterName,
                optionConfigValues,
                parameterId => { UseDefaultConfig = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.ConfigDateTimeParameterName,
                optionConfigValues,
                parameterId => { ConfigDateTime = optionConfigValues.DateTimeValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.ConfigCompleteParameterName,
                optionConfigValues,
                parameterId => { ConfigComplete = optionConfigValues.BooleanValue(parameterId); });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CHE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CHE006);
        }
    }
}