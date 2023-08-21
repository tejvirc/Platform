namespace Aristocrat.Monaco.G2S.Handlers.Printer
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts;
    using Hardware.Contracts.Printer;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{IPrinterDevice,printerStatus}" />
    /// </summary>
    public class PrinterStatusCommandBuilder : ICommandBuilder<IPrinterDevice, printerStatus>
    {
        private readonly IDeviceRegistryService _deviceRegistry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterStatusCommandBuilder" /> class.
        /// </summary>
        /// <param name="deviceRegistry">An <see cref="IDeviceRegistryService" /> instance.</param>
        public PrinterStatusCommandBuilder(IDeviceRegistryService deviceRegistry)
        {
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
        }

        /// <inheritdoc />
        public Task Build(IPrinterDevice device, printerStatus command)
        {
            var printer = _deviceRegistry.GetDevice<IPrinter>();

            command.configurationId = device.ConfigurationId;
            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;
            command.disconnected = !printer?.Connected ?? false;

            var faults = printer?.Faults ?? PrinterFaultTypes.None;
            var warnings = printer?.Warnings ?? PrinterWarningTypes.None;
            command.firmwareFault = faults.HasFlag(PrinterFaultTypes.FirmwareFault);
            command.mechanicalFault = faults.HasFlag(PrinterFaultTypes.OtherFault);
            command.opticalFault = faults.HasFlag(PrinterFaultTypes.PrintHeadDamaged);
            command.componentFault = faults.HasFlag(PrinterFaultTypes.TemperatureFault);
            command.nvMemoryFault = faults.HasFlag(PrinterFaultTypes.NvmFault);
            command.illegalActivity = false;
            command.chassisOpen = faults.HasFlag(PrinterFaultTypes.ChassisOpen);
            command.printHeadOpen = faults.HasFlag(PrinterFaultTypes.PrintHeadOpen);
            command.paperJam = faults.HasFlag(PrinterFaultTypes.PaperJam);
            command.paperLow = warnings.HasFlag(PrinterWarningTypes.PaperLow);
            command.paperEmpty = faults.HasFlag(PrinterFaultTypes.PaperEmpty);
            command.notAtTopOfForm = faults.HasFlag(PrinterFaultTypes.PaperNotTopOfForm);

            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;

            return Task.CompletedTask;
        }
    }
}
