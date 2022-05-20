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
    ///     Handles the ID Reader <see cref="IdReaderTimeoutEvent" />
    /// </summary>
    public class IdReaderTimeoutConsumer : Consumes<IdReaderTimeoutEvent>
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdReaderTimeoutEvent" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{IPrinterDevice,printerStatus}" /> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public IdReaderTimeoutConsumer(
            IG2SEgm egm,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <summary>
        ///     Handle IdReader Timeout events
        /// </summary>
        /// <param name="theEvent">The Event to handle</param>
        public override void Consume(IdReaderTimeoutEvent theEvent)
        {
            var idReader = _egm.GetDevice<IIdReaderDevice>(theEvent.IdReaderId);

            var status = new idReaderStatus();
            _commandBuilder.Build(idReader, status);
            _eventLift.Report(idReader, EventCode.G2S_IDE107, idReader.DeviceList(status));
        }
    }
}
