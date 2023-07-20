namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Contracts;
    using Kernel;

    public class CheckBalanceCommandHandler : ICommandHandler<CheckBalance>
    {
        private readonly IPlayerBank _bank;
        private readonly IGameHistory _gameHistory;
        private readonly IGamePlayState _gamePlayState;
        private readonly IPropertiesManager _properties;

        public CheckBalanceCommandHandler(
            IPlayerBank bank,
            IGamePlayState gamePlayState,
            IGameHistory gameHistory,
            IPropertiesManager properties)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public void Handle(CheckBalance command)
        {
            var strategy = _properties.GetValue(GamingConstants.GameEndCashOutStrategy, CashOutStrategy.None);
            var limit = _properties.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue);

            var adjustedBalance = _bank.Balance - command.PendingAmountOut;

            if (adjustedBalance < limit)
            {
                return;
            }

            switch (strategy)
            {
                case CashOutStrategy.Partial:
                    var amount = limit * (adjustedBalance / limit);

                    if (_gamePlayState.InGameRound && _gameHistory.CurrentLog != null)
                    {
                        var traceId = Guid.NewGuid();
                        command.ForcedCashout =
                            _bank.CashOut(traceId, amount, TransferOutReason.CashOut, true, _gameHistory.CurrentLog.TransactionId);

                        if (command.ForcedCashout)
                        {
                            _gameHistory.AppendCashOut(new CashOutInfo { Amount = amount, TraceId = traceId });
                        }
                    }
                    else
                    {
                        command.ForcedCashout = _bank.CashOut(amount, true);
                    }

                    break;
                case CashOutStrategy.Full:
                    if (!_gamePlayState.InGameRound ||
                        _properties.GetValue(GamingConstants.MeterFreeGamesIndependently, false))
                    {
                        command.ForcedCashout = _bank.CashOut(true);
                    }

                    break;
            }
        }
    }
}