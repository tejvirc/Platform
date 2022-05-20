namespace Aristocrat.Monaco.G2S.Descriptors
{
    using System;
    using Aristocrat.G2S.Client.Devices;
    using Hardware.Contracts;
    using Hardware.Contracts.Printer;

    public class PrinterDescriptor : DeviceDescriptorBase, IDeviceDescriptor<IPrinterDevice>
    {
        private readonly IDeviceRegistryService _deviceRegistry;

        public PrinterDescriptor(IDeviceRegistryService deviceRegistry)
        {
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
        }

        public Descriptor GetDescriptor(IPrinterDevice device)
        {
            var printer = _deviceRegistry.GetDevice<IPrinter>();

            return printer == null ? null : base.GetDescriptor(printer);
        }
    }
}