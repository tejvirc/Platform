namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.Printer;
    using System;

    /// <summary>
    ///     Handles the Printer <see cref="PrinterHardwareWarningConsumer" />
    /// </summary>
    public class PrinterHardwareWarningConsumer : Consumes<HardwareWarningEvent>
    {
        private readonly ICommandBuilder<IPrinterDevice, printerStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterHardwareWarningConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public PrinterHardwareWarningConsumer(
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
        public override void Consume(HardwareWarningEvent theEvent)
        {
            var printer = _egm.GetDevice<IPrinterDevice>(theEvent.PrinterId);
            if (printer == null)
            {
                return;
            }

            var status = new printerStatus();
            _commandBuilder.Build(printer, status);

            foreach (PrinterWarningTypes value in Enum.GetValues(typeof(PrinterWarningTypes)))
            {
                if ((theEvent.Warning & value) != value)
                {
                    continue;
                }

                string eventCode;
                switch (value)
                {
                    case PrinterWarningTypes.PaperLow:
                        eventCode = EventCode.G2S_PTE206;
                        break;
                    default:
                        continue;
                }

                _eventLift.Report(
                    printer,
                    eventCode,
                    printer.DeviceList(status),
                    null);
            }
        }
    }
}
