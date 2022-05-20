namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Bonus;

    public class GetBonusLogStatus : ICommandHandler<bonus, getBonusLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionHistory _transactionHistory;

        public GetBonusLogStatus(IG2SEgm egm, ITransactionHistory transactionHistory)
        {
            _egm = egm;
            _transactionHistory = transactionHistory;
        }

        public async Task<Error> Verify(ClassCommand<bonus, getBonusLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IBonusDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<bonus, getBonusLogStatus> command)
        {
            _egm.GetDevice<IBonusDevice>(command.IClass.deviceId).NotifyActive();

            var transactions = _transactionHistory.RecallTransactions<BonusTransaction>();
            var response = command.GenerateResponse<bonusLogStatus>();

            response.Command.totalEntries = transactions.Count;
            if (transactions.Count > 0)
            {
                response.Command.lastSequence = transactions.Max(x => x.LogSequence);
            }

            await Task.CompletedTask;
        }
    }
}