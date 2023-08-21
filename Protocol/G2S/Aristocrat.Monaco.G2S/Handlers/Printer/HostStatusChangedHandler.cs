namespace Aristocrat.Monaco.G2S.Handlers.Printer
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;

    public class HostStatusChangedHandler : IStatusChangedHandler<IPrinterDevice>
    {
        private readonly ICommandBuilder<IPrinterDevice, printerStatus> _commandBuilder;
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            IDeviceRegistryService deviceRegistry,
            ICommandBuilder<IPrinterDevice, printerStatus> commandBuilder,
            IEventLift eventLift)
        {
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IPrinterDevice device)
        {
            var printer = _deviceRegistry.GetDevice<IPrinter>();

            // The disabled event is handled in the PrinterDisabledConsumer
            if (device.HostEnabled)
            {
                printer?.Enable(EnabledReasons.Backend);

                var status = new printerStatus();
                _commandBuilder.Build(device, status);
                _eventLift.Report(
                    device,
                    EventCode.G2S_PTE004,
                    device.DeviceList(status));
            }
            else
            {
                printer?.Disable(DisabledReasons.Backend);
            }
        }
    }
}