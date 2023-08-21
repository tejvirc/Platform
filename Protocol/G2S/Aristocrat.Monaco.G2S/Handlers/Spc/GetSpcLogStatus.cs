namespace Aristocrat.Monaco.G2S.Handlers.Spc
{
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Spc;

    public class GetSpcLogStatus : ICommandHandler<spc, getSpcLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionHistory _transactionHistory;

        public GetSpcLogStatus(IG2SEgm egm, ITransactionHistory transactionHistory)
        {
            _egm = egm;
            _transactionHistory = transactionHistory;
        }

        public async Task<Error> Verify(ClassCommand<spc, getSpcLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<ICentralDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<spc, getSpcLogStatus> command)
        {
            var transactions = _transactionHistory.RecallTransactions<SpcTransaction>();
            var response = command.GenerateResponse<spcLogStatus>();

            response.Command.totalEntries = transactions.Count;
            if (transactions.Count > 0)
            {
                response.Command.lastSequence = transactions.Max(x => x.LogSequence);
            }

            await Task.CompletedTask;
        }
    }
}
