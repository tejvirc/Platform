namespace Aristocrat.Monaco.Bingo.GameEndWin
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Central;
    using Kernel;
    using Localization.Properties;
    using log4net;

    public class GameEndWinBonusCreditsStrategy : IGameEndWinStrategy
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string GEWBonusIdPrefix = "Bingo_GEW";

        private readonly IEventBus _bus;
        private readonly IBonusHandler _bonus;
        private readonly IMessageDisplay _messages;
        private readonly ICentralProvider _centralProvider;
        private readonly IGameHistory _history;

        public GameEndWinBonusCreditsStrategy(
            IEventBus bus,
            IBonusHandler bonus,
            IMessageDisplay messages,
            ICentralProvider centralProvider,
            IGameHistory history)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _bonus = bonus ?? throw new ArgumentNullException(nameof(bonus));
            _messages = messages ?? throw new ArgumentNullException(nameof(messages));
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _history = history ?? throw new ArgumentNullException(nameof(history));

            _bus.Subscribe<CashOutStartedEvent>(this, _ => RemoveGewMessage());
            _bus.Subscribe<GamePlayInitiatedEvent>(this, _ => RemoveGewMessage());
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
                _bus.UnsubscribeAll(this);
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
                _bus.UnsubscribeAll(this);
            }
        }

        private void RemoveGewMessage()
        {
            var transaction = _centralProvider.Transactions.OrderByDescending(x => x.TransactionId)
                .FirstOrDefault(x => x.OutcomeState == OutcomeState.Acknowledged || x.OutcomeState == OutcomeState.Committed);
            if (transaction == null)
            {
                return;
            }

            var playedGame = _history.GetGameHistory()
                .FirstOrDefault(h => transaction.AssociatedTransactions.Contains(h.TransactionId));

            var gameEndWinMessage = Localizer.For(CultureFor.Player).FormatString(ResourceKeys.GameEndWinAward,
                playedGame?.GameWinBonus.CentsToDollars().FormattedCurrencyString());
            if (playedGame?.GameWinBonus == 0)
            {
                return;
            }

            _messages.RemoveMessage(new DisplayableMessage(
                () => gameEndWinMessage,
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                typeof(BonusAwardedEvent)));
        }
    }
}