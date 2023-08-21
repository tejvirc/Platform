namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Bonus;

    public class BonusStatusCommandBuilder : ICommandBuilder<IBonusDevice, bonusStatus>
    {
        private readonly IBonusHandler _bonusHandler;
        private readonly ITransactionHistory _transactions;

        public BonusStatusCommandBuilder(IBonusHandler bonusHandler, ITransactionHistory transactions)
        {
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
        }

        public Task Build(IBonusDevice device, bonusStatus command)
        {
            var logs = _transactions.RecallTransactions<BonusTransaction>();

            var egmEscrow = logs
                .Where(l => l.Mode == BonusMode.WagerMatch && l.State == BonusState.Pending && !l.IdRequired)
                .Sum(l => l.WagerMatchAwardAmount);
            var playerEscrow = logs
                .Where(l => l.Mode == BonusMode.WagerMatch && l.State == BonusState.Pending && l.IdRequired)
                .Sum(l => l.WagerMatchAwardAmount);

            command.bonusActive = device.BonusActive;
            command.configurationId = device.ConfigurationId;
            command.hostEnabled = device.HostEnabled;
            command.egmEnabled = device.Enabled;
            command.hostLocked = device.HostLocked;
            command.hostActive = device.HostActive;
            command.bonusActive = device.BonusActive;
            command.delayValue = (int)_bonusHandler.GameEndDelay.TotalMilliseconds;
            command.delayTime = (int)_bonusHandler.DelayDuration.TotalMilliseconds;
            command.delayGames = _bonusHandler.DelayedGames;
            command.delayLater = _bonusHandler.EvaluateBoth;
            command.wmEgmEscrow = egmEscrow;
            command.wmPlayerEscrow = playerEscrow;
            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;

            return Task.CompletedTask;
        }
    }
}