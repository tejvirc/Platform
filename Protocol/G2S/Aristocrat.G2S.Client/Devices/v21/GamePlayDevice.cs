namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     The <i>gamePlay</i> class includes commands and events related to the games accessible to a player on an EGM. The
    ///     class provides a means to discover the capabilities of each <i>gamePlay</i> device as well as control which
    ///     <i>gamePlay</i> devices are enabled and which associated game combos(defined below) are accessible to the player,
    ///     thus allowing the player to select them.The class provides commands to determine the state of <i>gamePlay</i>
    ///     devices and obtain a game recall log of current and previous games.
    /// </summary>
    /// <remarks>
    ///     The <i>gamePlay</i> class is a multi-device class. Class-level meters and logs <b>MUST</b> include activity related
    ///     to both active and inactive devices.Each <i>gamePlay</i> device is assigned its own unique deviceId.
    /// </remarks>
    public class GamePlayDevice : ClientDeviceBase<gamePlay>, IGamePlayDevice
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayDevice" /> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public GamePlayDevice(int deviceId, IDeviceObserver deviceObserver)
            : base(deviceId, deviceObserver, false)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public bool RestartStatus { get; private set; }

        /// <inheritdoc />
        public bool SetViaAccessConfig { get; private set; }

        /// <inheritdoc />
        public string DenomMeterType { get; private set; }

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

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE006);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE007);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE008);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE099);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE104); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE107);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE108);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE109);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE110);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE111);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE112);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE113);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE201);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_GPE202);
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

            SetViaAccessConfig = false;
            DenomMeterType = "G2S_eachDenom";
        }
    }
}