namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.IdReader;

    /// <summary>
    ///     Handles the ID Reader <see cref="HardwareFaultEvent" />
    /// </summary>
    public class IdReaderHardwareFaultConsumer : Consumes<HardwareFaultEvent>
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdReaderHardwareFaultConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{IPrinterDevice,printerStatus}" /> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public IdReaderHardwareFaultConsumer(
            IG2SEgm egm,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> commandBuilder,
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
            var idReader = _egm.GetDevice<IIdReaderDevice>(theEvent.IdReaderId);

            if (idReader == null || !idReader.Enabled)
            {
                return;
            }

            idReader.Enabled = false;

            foreach (IdReaderFaultTypes value in Enum.GetValues(typeof(IdReaderFaultTypes)))
            {
                string eventCode;

                if ((theEvent.Fault & value) != value)
                {
                    continue;
                }

                switch (value)
                {
                    case IdReaderFaultTypes.ComponentFault:
                        eventCode = EventCode.G2S_IDE906;
                        break;
                    case IdReaderFaultTypes.PowerFail:
                        eventCode = EventCode.G2S_IDE904;
                        break;
                    case IdReaderFaultTypes.FirmwareFault:
                        eventCode = EventCode.G2S_IDE903;
                        break;
                    default:
                        continue;
                }

                if (!string.IsNullOrEmpty(eventCode))
                {
                    var status = new idReaderStatus();
                    _commandBuilder.Build(idReader, status);
                    _eventLift.Report(idReader, eventCode, idReader.DeviceList(status), theEvent);
                }
            }
        }
    }
}
