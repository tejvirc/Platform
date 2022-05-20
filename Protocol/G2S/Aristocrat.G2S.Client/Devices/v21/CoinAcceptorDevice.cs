namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     The <i>coinAcceptor</i> class includes commands and events related to coin acceptors and related equipment within
    ///     an EGM.
    /// </summary>
    /// <remarks>
    ///     The class includes the command used to query the state of the coin acceptor. It also includes commands used to
    ///     enable and disable the coin acceptor and to query the types of coins accepted by the coin acceptor.
    /// </remarks>
    public class CoinAcceptorDevice : ClientDeviceBase<coinAcceptor>, ICoinAcceptor
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinAcceptorDevice" /> class.
        /// </summary>
        /// <param name="deviceObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public CoinAcceptorDevice(IDeviceObserver deviceObserver)
            : base(1, deviceObserver, false)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public bool RestartStatus { get; private set; }

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

            // TODO: We don't really support this class.  It's just a stub
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE006);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE099);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE107);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE108);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE109);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE110);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE901);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE902);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE903);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE904);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE905);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE906);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE907);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CAE908);
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
        }
    }
}