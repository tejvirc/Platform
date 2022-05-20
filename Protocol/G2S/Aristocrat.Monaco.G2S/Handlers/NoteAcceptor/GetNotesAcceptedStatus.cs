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
    ///     Implementation of 'getNotesAcceptedStatus' command of 'NoteAcceptor' G2S class.
    /// </summary>
    public class GetNotesAcceptedStatus : ICommandHandler<noteAcceptor, getNotesAcceptedStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetNotesAcceptedStatus" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm.</param>
        /// <param name="transactionHistory">An <see cref="ITransactionHistory" /> instance.</param>
        public GetNotesAcceptedStatus(IG2SEgm egm, ITransactionHistory transactionHistory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<noteAcceptor, getNotesAcceptedStatus> command)
        {
            var transactions = _transactionHistory.RecallTransactions<BillTransaction>();

            var response = command.GenerateResponse<notesAcceptedStatus>();

            response.Command.totalEntries = transactions.Count;
            if (transactions.Count != 0)
            {
                response.Command.lastSequence = transactions.Max(x => x.LogSequence);
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<noteAcceptor, getNotesAcceptedStatus> command)
        {
            return await Sanction.OwnerAndGuests<INoteAcceptorDevice>(_egm, command);
        }
    }
}