namespace Aristocrat.Monaco.G2S.Handlers.Central
{
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Central;

    public class GetCentralLogStatus : ICommandHandler<central, getCentralLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionHistory _transactionHistory;

        public GetCentralLogStatus(IG2SEgm egm, ITransactionHistory transactionHistory)
        {
            _egm = egm;
            _transactionHistory = transactionHistory;
        }

        public async Task<Error> Verify(ClassCommand<central, getCentralLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<ICentralDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<central, getCentralLogStatus> command)
        {
            var transactions = _transactionHistory.RecallTransactions<CentralTransaction>();
            var response = command.GenerateResponse<centralLogStatus>();

            response.Command.totalEntries = transactions.Count;
            if (transactions.Count > 0)
            {
                response.Command.lastSequence = transactions.Max(x => x.LogSequence);
            }

            await Task.CompletedTask;
        }
    }
}