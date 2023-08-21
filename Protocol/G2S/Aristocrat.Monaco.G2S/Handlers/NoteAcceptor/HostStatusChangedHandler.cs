namespace Aristocrat.Monaco.G2S.Handlers.NoteAcceptor
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;

    public class HostStatusChangedHandler : IStatusChangedHandler<INoteAcceptorDevice>
    {
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _commandBuilder;
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> commandBuilder,
            IEventLift eventLift,
            IDeviceRegistryService deviceRegistry)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
        }

        public void Handle(INoteAcceptorDevice device)
        {
            var noteAcceptor = _deviceRegistry.GetDevice<INoteAcceptor>();
            if (device.HostEnabled)
            {
                noteAcceptor?.Enable(EnabledReasons.Backend);
            }
            else
            {
                noteAcceptor?.Disable(DisabledReasons.Backend);
            }

            var status = new noteAcceptorStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_NAE004 : EventCode.G2S_NAE003,
                device.DeviceList(status));
        }
    }
}