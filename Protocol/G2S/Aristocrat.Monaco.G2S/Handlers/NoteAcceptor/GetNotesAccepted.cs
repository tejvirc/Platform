namespace Aristocrat.Monaco.G2S.Handlers.NoteAcceptor
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Implementation of 'getNotesAccepted' command of 'NoteAcceptor' G2S class.
    /// </summary>
    public class GetNotesAccepted : ICommandHandler<noteAcceptor, getNotesAccepted>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetNotesAccepted" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm.</param>
        /// <param name="transactionHistory">An <see cref="ITransactionHistory" /> instance.</param>
        public GetNotesAccepted(IG2SEgm egm, ITransactionHistory transactionHistory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<noteAcceptor, getNotesAccepted> command)
        {
            return await Sanction.OwnerAndGuests<INoteAcceptorDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<noteAcceptor, getNotesAccepted> command)
        {
            var response = command.GenerateResponse<notesAcceptedList>();

            var billTransactions = _transactionHistory.RecallTransactions<BillTransaction>();

            response.Command.notesAcceptedLog = billTransactions
                .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                .Select(transaction => transaction.ToNotesAcceptedLog()).ToArray();

            await Task.CompletedTask;
        }
    }
}