namespace Aristocrat.Monaco.Bingo.GameEndWin
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Gaming.Contracts.Bonus;
    using Kernel;
    using Localization.Properties;
    using log4net;

    public class GameEndWinBonusCreditsStrategy : IGameEndWinStrategy
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private const string GEWBonusIdPrefix = "Bingo_GEW";

        private readonly IEventBus _bus;
        private readonly IBonusHandler _bonus;

        public GameEndWinBonusCreditsStrategy(
            IEventBus bus,
            IBonusHandler bonus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _bonus = bonus ?? throw new ArgumentNullException(nameof(bonus));
        }

        public async Task<bool> ProcessWin(long winAmount, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var bonusId = $"{GEWBonusIdPrefix}_{Guid.NewGuid()}";
                var pending = new TaskCompletionSource<bool>();

                using var register = token.Register(TryCancel);
                _bus.Subscribe<BonusAwardedEvent>(
                    this,
                    _ => pending.TrySetResult(true),
                    evt => evt.Transaction.BonusId == bonusId);
                _bus.Subscribe<BonusFailedEvent>(
                    this,
                    _ => pending.TrySetResult(false),
                    evt => evt.Transaction.BonusId == bonusId);
                var award = _bonus.Award(
                    new GameWinBonus(
                        bonusId,
                        winAmount,
                        0,
                        0,
                        PayMethod.Any,
                        protocol: CommsProtocol.Bingo)
                    {
                        Message = Localizer.For(CultureFor.Player).FormatString(
                            ResourceKeys.GameEndWinAward,
                            winAmount.MillicentsToDollars().FormattedCurrencyString())
                    });
                if (award is null)
                {
                    pending.TrySetResult(false);
                }

                return await pending.Task;
                void TryCancel()
                {
                    var isCancelled = _bonus.Cancel(bonusId);
                    if (isCancelled)
                    {
                        pending.TrySetCanceled();
                    }

                    Logger.Warn($"GEW Bonus attempted to cancel.  IsCancelled={isCancelled}");
                }
            }
            finally
            {
                // ReSharper disable once MethodSupportsCancellation
                // Purposely not providing the token here as this must complete
                await Task.Run(() => _bus.UnsubscribeAll(this));
            }
        }

        public async Task<bool> Recover(long gameTransactionId, CancellationToken token)
        {
            try
            {
                var pending = new TaskCompletionSource<bool>();
                using var register = token.Register(() => pending.TrySetCanceled());
                var award = _bonus.Transactions.FirstOrDefault(
                    x => x.Mode is BonusMode.GameWin && x.AssociatedTransactions.Contains(gameTransactionId));
                _bus.Subscribe<BonusAwardedEvent>(
                    this,
                    _ => pending.TrySetResult(true),
                    evt => evt.Transaction.BonusId == award?.BonusId);
                _bus.Subscribe<BonusFailedEvent>(
                    this,
                    _ => pending.TrySetResult(false),
                    evt => evt.Transaction.BonusId == award?.BonusId);
                if (award is null || award.State >= BonusState.Committed)
                {
                    pending.TrySetResult((award?.PaidAmount ?? 0) > 0);
                }

                return await pending.Task;
            }
            finally
            {
                // ReSharper disable once MethodSupportsCancellation
                // Purposely not providing the token here as this must complete
                await Task.Run(() => _bus.UnsubscribeAll(this));
            }
        }
    }
}