namespace Aristocrat.Monaco.G2S.Handlers.Central
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
    using Gaming.Contracts.Central;

    public class GetCentralLog : ICommandHandler<central, getCentralLog>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameProvider _games;
        private readonly ITransactionHistory _transactionHistory;

        public GetCentralLog(IG2SEgm egm, IGameProvider games, ITransactionHistory transactionHistory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        public async Task<Error> Verify(ClassCommand<central, getCentralLog> command)
        {
            return await Sanction.OwnerAndGuests<ICentralDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<central, getCentralLog> command)
        {
            var response = command.GenerateResponse<centralLogList>();

            var transactions = _transactionHistory.RecallTransactions<CentralTransaction>();

            response.Command.centralLog = transactions
                .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                .Select(transaction => transaction.ToCentralLog(_games)).ToArray();

            await Task.CompletedTask;
        }
    }
}