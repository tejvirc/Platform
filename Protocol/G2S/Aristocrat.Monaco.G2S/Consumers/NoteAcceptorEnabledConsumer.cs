namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.NoteAcceptor;

    /// <summary>
    ///     Handles the Note Acceptor <see cref="EnabledEvent" />
    /// </summary>
    public class NoteAcceptorEnabledConsumer : Consumes<EnabledEvent>
    {
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorEnabledConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{INoteAcceptorDevice,noteAcceptorStatus}" /> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public NoteAcceptorEnabledConsumer(
            IG2SEgm egm,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(EnabledEvent theEvent)
        {
            var noteAcceptor = _egm.GetDevice<INoteAcceptorDevice>();
            if (noteAcceptor == null || noteAcceptor.Enabled)
            {
                return;
            }

            noteAcceptor.Enabled = true;

            _egm.GetDevice<ICabinetDevice>().RemoveConditions(noteAcceptor);

            var status = new noteAcceptorStatus();
            _commandBuilder.Build(noteAcceptor, status);
            _eventLift.Report(
                noteAcceptor,
                EventCode.G2S_NAE002,
                noteAcceptor.DeviceList(status),
                theEvent);
        }
    }
}