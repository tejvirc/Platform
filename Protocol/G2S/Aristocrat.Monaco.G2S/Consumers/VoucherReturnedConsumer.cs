namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.NoteAcceptor;

    /// <inheritdoc />
    /// <summary>
    ///     Handles the Note Acceptor <see cref="T:Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor.VoucherReturnedEvent" />
    /// </summary>
    public class VoucherReturnedConsumer : NoteAcceptorConsumerBase<VoucherReturnedEvent>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Aristocrat.Monaco.G2S.Consumers.VoucherReturnedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public VoucherReturnedConsumer(
            IG2SEgm egm,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> commandBuilder,
            IEventLift eventLift)
            : base(egm, commandBuilder, eventLift, EventCode.G2S_NAE108)
        {
        }

        /// <inheritdoc />
        protected override string GetEventCode(VoucherReturnedEvent theEvent)
        {
            return EventCode.G2S_NAE109;
        }
    }
}