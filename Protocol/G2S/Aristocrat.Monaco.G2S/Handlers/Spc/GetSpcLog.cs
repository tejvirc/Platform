namespace Aristocrat.Monaco.G2S.Handlers.Spc
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Spc;

    public class GetSpcLog : ICommandHandler<spc, getSpcLog>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameProvider _games;
        private readonly ITransactionHistory _transactionHistory;

        public GetSpcLog(IG2SEgm egm, IGameProvider games, ITransactionHistory transactionHistory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        public async Task<Error> Verify(ClassCommand<spc, getSpcLog> command)
        {
            return await Sanction.OwnerAndGuests<ISpcDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<spc, getSpcLog> command)
        {
            var response = command.GenerateResponse<spcLogList>();

            var transactions = _transactionHistory.RecallTransactions<SpcTransaction>();

            response.Command.spcLog = transactions
                .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                .Select(transaction => transaction.ToSpcLog(_games))
                .ToArray();

            await Task.CompletedTask;
        }
    }
}