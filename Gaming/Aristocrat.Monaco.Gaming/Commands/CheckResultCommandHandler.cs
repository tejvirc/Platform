namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Accounting.Contracts.TransferOut;
    using Contracts;
    using Kernel;
    using Progressives;

    public class CheckResultCommandHandler : ICommandHandler<CheckResult>
    {
        private readonly IPlayerBank _bank;
        private readonly IPropertiesManager _properties;
        private readonly IGameHistory _gameHistory;
        private readonly IProgressiveConfigurationProvider _progressiveProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CheckResultCommandHandler"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="bank">The bank.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="gameHistory">The game history.</param>
        /// <param name="progressiveProvider">The progressive provider.</param>
        public CheckResultCommandHandler(
            IPlayerBank bank,
            IPropertiesManager properties,
            IGameHistory gameHistory,
            IProgressiveConfigurationProvider progressiveProvider)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _progressiveProvider = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
        }

        /// <inheritdoc/>
        public void Handle(CheckResult command)
        {
            var checkMaxWin = true;

            var (game, denomination) = _properties.GetActiveGame();

            var log = _gameHistory.CurrentLog;

            var result = command.Result * GamingConstants.Millicents;

            var strategy = _properties.GetValue(GamingConstants.GameEndCashOutStrategy, CashOutStrategy.None);
            if (strategy == CashOutStrategy.Full)
            {
                CheckMaxWin(checkMaxWin, result, true);
                return;
            }

            command.AmountOut = 0;

            foreach (var jackpot in log.Jackpots)
            {
                // Skip this jackpot if it's already been handled
                if (log.CashOutInfo.Any(c => c.AssociatedTransactions.Contains(jackpot.TransactionId)))
                {
                    continue;
                }

                var level = _progressiveProvider.GetProgressiveLevel(jackpot, game.Id, denomination.Value);
                if (level == null || level.AllowTruncation)
                {
                    continue;
                }

                var traceId = Guid.NewGuid();

                command.ForcedCashout = _bank.ForceHandpay(traceId, jackpot.WinAmount, TransferOutReason.LargeWin, log.TransactionId);
                if (command.ForcedCashout)
                {
                    command.AmountOut += jackpot.WinAmount;
                    _gameHistory.AppendCashOut(
                        new CashOutInfo
                        {
                            Amount = jackpot.WinAmount,
                            Reason = TransferOutReason.LargeWin,
                            TraceId = traceId,
                            AssociatedTransactions = new[] { jackpot.TransactionId },
                            Handpay = true
                        });
                }

                checkMaxWin = false;
            }

            CheckMaxWin(checkMaxWin, result);

            void CheckMaxWin(bool checkWin, long winAmount, bool fullCashOut = false)
            {
                if (checkWin && game.MaximumWinAmount > 0 && winAmount >= game.MaximumWinAmount)
                {
                    var traceId = Guid.NewGuid();

                    var amount = fullCashOut ? _bank.Balance : winAmount;

                    command.ForcedCashout = _bank.CashOut(traceId, amount, TransferOutReason.CashOut, true, log.TransactionId, true);
                    if (command.ForcedCashout)
                    {
                        command.AmountOut += amount;
                        _gameHistory.AppendCashOut(new CashOutInfo { Amount = amount, TraceId = traceId });
                    }
                }
            }
        }
    }
}