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
    ///     Handles the Printer <see cref="PrinterHardwareFaultConsumer" />
    /// </summary>
    public class PrinterHardwareFaultConsumer : Consumes<HardwareFaultEvent>
    {
        private readonly ICommandBuilder<IPrinterDevice, printerStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterHardwareFaultConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public PrinterHardwareFaultConsumer(
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
        public override void Consume(HardwareFaultEvent theEvent)
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
                    case PrinterFaultTypes.FirmwareFault:
                        eventCode = EventCode.G2S_PTE903;
                        break;
                    case PrinterFaultTypes.TemperatureFault:
                    case PrinterFaultTypes.OtherFault:
                        eventCode = EventCode.G2S_PTE904;
                        break;
                    case PrinterFaultTypes.PrintHeadDamaged:
                        eventCode = EventCode.G2S_PTE905;
                        break;
                    case PrinterFaultTypes.NvmFault:
                        eventCode = EventCode.G2S_PTE907;
                        break;
                    case PrinterFaultTypes.PaperJam:
                        eventCode = EventCode.G2S_PTE205;
                        break;
                    case PrinterFaultTypes.PaperEmpty:
                        eventCode = EventCode.G2S_PTE207;
                        break;
                    case PrinterFaultTypes.PaperNotTopOfForm:
                        eventCode = EventCode.G2S_PTE208;
                        break;
                    case PrinterFaultTypes.PrintHeadOpen:
                        eventCode = EventCode.G2S_PTE203;
                        break;
                    case PrinterFaultTypes.ChassisOpen:
                        eventCode = EventCode.G2S_PTE201;
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
        }
    }
}
