namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     The noteAcceptor class includes commands and events related to note acceptors and related equipment
    ///     within an EGM. The class includes commands used to query the state of the note acceptor and the log of
    ///     notes accepted. It also includes commands used to enable or disable the note acceptor and to query the types
    ///     of notes accepted by the device
    /// </summary>
    public class NoteAcceptorDevice : ClientDeviceBase<noteAcceptor>, INoteAcceptorDevice
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorDevice" /> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public NoteAcceptorDevice(int deviceId, IDeviceObserver deviceObserver)
            : base(deviceId, deviceObserver)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public bool RestartStatus { get; set; }

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

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE006);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE099);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE101);
            EventHandlerDevice.RegisterEvent(
                deviceClass,
                Id,
                EventCode.G2S_NAE102); //// TODO: 107 currently covers both
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE107);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE108);
            EventHandlerDevice.RegisterEvent(
                deviceClass,
                Id,
                EventCode.G2S_NAE109); //// TODO: 108 currently covers both
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE110);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE111); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE112);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE113);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE114);
            EventHandlerDevice.RegisterEvent(
                deviceClass,
                Id,
                EventCode.G2S_NAE115); //// TODO: 114 currently covers both
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE116);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE117); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE118);
            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE119);  //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE901);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE902);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE903);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE904);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE905);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE906);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE907);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_NAE908);
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