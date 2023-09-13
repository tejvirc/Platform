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
        private readonly IGameProvider _gameProvider;
        private readonly IPropertiesManager _properties;
        private readonly IGameHistory _gameHistory;
        private readonly IProgressiveConfigurationProvider _progressiveProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CheckResultCommandHandler"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="bank">The bank.</param>
        /// <param name="gameProvider">The game provider.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="gameHistory">The game history.</param>
        /// <param name="progressiveProvider">The progressive provider.</param>
        public CheckResultCommandHandler(
            IPlayerBank bank,
            IGameProvider gameProvider,
            IPropertiesManager properties,
            IGameHistory gameHistory,
            IProgressiveConfigurationProvider progressiveProvider)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _progressiveProvider = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
        }

        /// <inheritdoc/>
        public void Handle(CheckResult command)
        {
            var strategy = _properties.GetValue(GamingConstants.GameEndCashOutStrategy, CashOutStrategy.None);
            if (strategy == CashOutStrategy.Full)
            {
                return;
            }

            var (game, denomination) = _gameProvider.GetActiveGame();

            var result = command.Result * GamingConstants.Millicents;
            var checkMaxWin = true;

            var log = _gameHistory.CurrentLog;

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

            if (checkMaxWin && game.MaximumWinAmount > 0 && result >= game.MaximumWinAmount)
            {
                var traceId = Guid.NewGuid();

                command.ForcedCashout = _bank.CashOut(traceId, result, TransferOutReason.CashOut, true, log.TransactionId);
                if (command.ForcedCashout)
                {
                    command.AmountOut += result;
                    _gameHistory.AppendCashOut(new CashOutInfo { Amount = result, TraceId = traceId });
                }
            }
        }
    }
}
