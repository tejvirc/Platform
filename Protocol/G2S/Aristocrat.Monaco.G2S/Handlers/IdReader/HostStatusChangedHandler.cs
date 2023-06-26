namespace Aristocrat.Monaco.G2S.Handlers.IdReader
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.SharedDevice;

    public class HostStatusChangedHandler : IStatusChangedHandler<IIdReaderDevice>
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _commandBuilder;
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            IDeviceRegistryService deviceRegistry,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> commandBuilder,
            IEventLift eventLift)
        {
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IIdReaderDevice device)
        {
            var provider = _deviceRegistry.GetDevice<IIdReaderProvider>();
            var reader = provider?[device.Id];
            if (device.HostEnabled)
            {
                reader?.Enable(EnabledReasons.Backend);
            }
            else
            {
                reader?.Disable(DisabledReasons.Backend);
            }

            // The disabled event is handled in IdReaderEnabledConsumer
            if (device.HostEnabled)
            {
                var status = new idReaderStatus();
                _commandBuilder.Build(device, status);
                _eventLift.Report(device, EventCode.G2S_IDE004, device.DeviceList(status));
            }
        }
    }
}