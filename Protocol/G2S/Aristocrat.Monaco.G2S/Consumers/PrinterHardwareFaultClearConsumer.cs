namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.Printer;
    using Kernel;
    using System;

    /// <summary>
    ///     Handles the Printer <see cref="PrinterHardwareFaultClearConsumer"/>
    /// </summary>
    public class PrinterHardwareFaultClearConsumer : Consumes<HardwareFaultClearEvent>
    {
        private readonly ICommandBuilder<IPrinterDevice, printerStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterHardwareFaultClearConsumer"/> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm"/> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}"/> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public PrinterHardwareFaultClearConsumer(
            IG2SEgm egm,
            ICommandBuilder<IPrinterDevice, printerStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <summary>
        ///     Handle hardware fault events
        /// </summary>
        /// <param name="theEvent">The Event to handle</param>
        public override void Consume(HardwareFaultClearEvent theEvent)
        {
            var device = _egm.GetDevice<IPrinterDevice>(theEvent.PrinterId);
            if (device == null)
            {
                return;
            }

            var status = new printerStatus();
            _commandBuilder.Build(device, status);

            foreach (PrinterFaultTypes value in Enum.GetValues(typeof(PrinterFaultTypes)))
            {
                if ((theEvent.Fault & value) != value)
                {
                    continue;
                }

                string eventCode;
                switch (value)
                {
                    case PrinterFaultTypes.PaperEmpty:
                        eventCode = EventCode.G2S_PTE209;
                        break;
                    case PrinterFaultTypes.PrintHeadOpen:
                        eventCode = EventCode.G2S_PTE204;
                        break;
                    case PrinterFaultTypes.ChassisOpen:
                        eventCode = EventCode.G2S_PTE202;
                        break;
                    default:
                        continue;
                }

                _eventLift.Report(
                    device,
                    eventCode,
                    device.DeviceList(status),
                    null);
            }

            var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
            if ((printer?.Faults ?? PrinterFaultTypes.None) == PrinterFaultTypes.None)
            {
                _eventLift.Report(
                    device,
                    EventCode.G2S_PTE099,
                    device.DeviceList(status),
                    null);
            }
        }
    }
}
