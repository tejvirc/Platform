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
    ///     Handles the Printer <see cref="EnabledEvent" />
    /// </summary>
    public class PrinterEnabledConsumer : Consumes<EnabledEvent>
    {
        private readonly ICommandBuilder<IPrinterDevice, printerStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterEnabledConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{IPrinterDevice,printerStatus}" /> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public PrinterEnabledConsumer(
            IG2SEgm egm,
            ICommandBuilder<IPrinterDevice, printerStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(EnabledEvent theEvent)
        {
            if ((theEvent.Reasons & EnabledReasons.Service) == EnabledReasons.Service ||
                (theEvent.Reasons & EnabledReasons.Reset) == EnabledReasons.Reset ||
                (theEvent.Reasons & EnabledReasons.Device) == EnabledReasons.Device)
            {
                var printer = _egm.GetDevice<IPrinterDevice>(theEvent.PrinterId);
                if (printer == null || printer.Enabled)
                {
                    return;
                }

                printer.Enabled = true;

                _egm.GetDevice<ICabinetDevice>().RemoveCondition(printer, EgmState.EgmDisabled);

                var status = new printerStatus();
                _commandBuilder.Build(printer, status);
                _eventLift.Report(
                    printer,
                    EventCode.G2S_PTE002,
                    printer.DeviceList(status));

                if (theEvent.Reasons.HasFlag(EnabledReasons.Reset))
                {
                    _commandBuilder.Build(printer, status);
                    _eventLift.Report(
                        printer,
                        EventCode.G2S_PTE099,
                        printer.DeviceList(status));
                }
            }
        }
    }
}