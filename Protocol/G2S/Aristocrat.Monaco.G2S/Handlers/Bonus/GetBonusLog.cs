namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Bonus;

    public class GetBonusLog : ICommandHandler<bonus, getBonusLog>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionHistory _transactionHistory;

        public GetBonusLog(IG2SEgm egm, ITransactionHistory transactionHistory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        public async Task<Error> Verify(ClassCommand<bonus, getBonusLog> command)
        {
            return await Sanction.OwnerAndGuests<IBonusDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<bonus, getBonusLog> command)
        {
            _egm.GetDevice<IBonusDevice>(command.IClass.deviceId).NotifyActive();

            var response = command.GenerateResponse<bonusLogList>();

            var transactions = _transactionHistory.RecallTransactions<BonusTransaction>();

            response.Command.bonusLog = transactions
                .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                .Select(transaction => transaction.ToBonusLog()).ToArray();

            await Task.CompletedTask;
        }
    }
}