namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;

    /// <summary>
    ///     Handles the Printer <see cref="DisabledEvent" />
    /// </summary>
    public class PrinterDisabledConsumer : Consumes<DisabledEvent>
    {
        private readonly ICommandBuilder<IPrinterDevice, printerStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterDisabledConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{IPrinterDevice,printerStatus}" /> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public PrinterDisabledConsumer(
            IG2SEgm egm,
            ICommandBuilder<IPrinterDevice, printerStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(DisabledEvent theEvent)
        {
            var printer = _egm.GetDevice<IPrinterDevice>(theEvent.PrinterId);
            if (printer == null || !printer.Enabled)
            {
                return;
            }

            var eventCode = string.Empty;

            if ((theEvent.Reasons & DisabledReasons.Service) == DisabledReasons.Service ||
                (theEvent.Reasons & DisabledReasons.Error) == DisabledReasons.Error ||
                (theEvent.Reasons & DisabledReasons.FirmwareUpdate) == DisabledReasons.FirmwareUpdate ||
                (theEvent.Reasons & DisabledReasons.Device) == DisabledReasons.Device)
            {
                printer.Enabled = false;

                _egm.GetDevice<ICabinetDevice>().AddCondition(printer, EgmState.EgmDisabled);

                eventCode = EventCode.G2S_PTE001;
            }
            else if ((theEvent.Reasons & DisabledReasons.Backend) == DisabledReasons.Backend)
            {
                eventCode = EventCode.G2S_PTE003;
            }

            if (!string.IsNullOrEmpty(eventCode))
            {
                var status = new printerStatus();
                _commandBuilder.Build(printer, status);
                _eventLift.Report(
                    printer,
                    eventCode,
                    printer.DeviceList(status));
            }
        }
    }
}
