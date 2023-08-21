namespace Aristocrat.G2S.Client.Devices.v21
{
    using Newtonsoft.Json;
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with and control a MediaDisplay device.
    /// </summary>
    public class MediaDisplayDevice : ClientDeviceBase<mediaDisplay>, IMediaDisplay
    {
        private const string IgtPrefix = @"IGT_";

        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaDisplayDevice" /> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public MediaDisplayDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver, false)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public bool RestartStatus { get; private set; }

        /// <inheritdoc />
        public override string DevicePrefix => IgtPrefix;

        /// <inheritdoc />
        [JsonIgnore]
        public IClass MediaDisplayClassInstance => InternalCreateClass();

        /// <inheritdoc />
        public int TimeToLive { get; private set; }

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

            SetDeviceValue(
                G2SParametersNames.TimeToLiveParameterName,
                optionConfigValues,
                parameterId => { TimeToLive = optionConfigValues.Int32Value(parameterId); });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            //EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE001); // not used
            //EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE002); // not used
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE005);
            //EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE006); // not used
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE007);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE008);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE107);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE108);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE109);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.IGT_MDE110);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_MDE111);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_MDE112);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_MDE113);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_MDE114);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_MDE115);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_MDE116);
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
            TimeToLive = (int)Constants.DefaultTimeout.TotalMilliseconds;
        }
    }
}
