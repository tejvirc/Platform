namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Commands;
    using Common.Events;
    using Common.GameOverlay;
    using Common.Storage;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Central;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Protocol.Common.Storage.Entity;
    using GameEndWinFactory =
        Common.IBingoStrategyFactory<GameEndWin.IGameEndWinStrategy, Common.Storage.Model.GameEndWinStrategy>;

    public class BingoReplayRecovery : IBingoReplayRecovery, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _bus;
        private readonly ICentralProvider _centralProvider;
        private readonly IPropertiesManager _properties;
        private readonly IGameHistory _history;
        private readonly IMessageDisplay _messages;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IUnitOfWorkFactory _unitOfWork;
        private readonly GameEndWinFactory _gewFactory;
        private readonly IBonusHandler _bonusHandler;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IGamePlayState _gamePlayState;

        private Guid _recoveryMessageId;
        private long _recoveredTransactionId;

        private bool _disposed;

        public BingoReplayRecovery(
            IEventBus bus,
            ICentralProvider centralProvider,
            IPropertiesManager properties,
            IGameHistory history,
            IMessageDisplay messages,
            ICommandHandlerFactory commandFactory,
            IUnitOfWorkFactory unitOfWork,
            GameEndWinFactory gewFactory,
            IBonusHandler bonusHandler,
            ITransactionHistory transactionHistory,
            IGamePlayState gamePlayState)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _messages = messages ?? throw new ArgumentNullException(nameof(messages));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _gewFactory = gewFactory ?? throw new ArgumentNullException(nameof(gewFactory));
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));

            _bus.Subscribe<GamePlayInitiatedEvent>(this, _ => ClearGameEndWinMessage());
            _bus.Subscribe<BankBalanceChangedEvent>(this, Handle);
        }

        public Task RecoverDisplay(CancellationToken token)
        {
            RecoverBingoDisplay(GetLastTransaction(), false, true);
            return Task.CompletedTask;
        }

        public async Task RecoverGamePlay(CancellationToken token)
        {
            await RecoveryBingoGamePlay(GetLastTransaction(), token);
        }

        public Task Replay(IGameHistoryLog log, bool finalizeReplay, CancellationToken token)
        {
            var transaction = _centralProvider.Transactions.FirstOrDefault(x => x.AssociatedTransactions.Contains(log.TransactionId));
            if (finalizeReplay)
            {
                if (transaction?.Descriptions?.FirstOrDefault() is not BingoGameDescription bingoGame)
                {
                    return Task.CompletedTask;
                }

                RecoverBallCall(false, true, false, bingoGame);
            }
            else
            {
                RecoverBingoDisplay(transaction, true, false);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private static int GetDaubs(BingoCard card, bool initialBallCall) =>
            initialBallCall && card.InitialDaubedBits.HasValue ? card.InitialDaubedBits.Value : card.DaubedBits;

        private CentralTransaction GetLastTransaction()
        {
            return _centralProvider.Transactions.OrderByDescending(x => x.TransactionId)
                .FirstOrDefault(x => x.OutcomeState >= OutcomeState.Committed);
        }

        private IGameHistoryLog GetGameHistory(ITransactionConnector transaction)
        {
            return _history.GetGameHistory()
                .FirstOrDefault(h => transaction.AssociatedTransactions.Contains(h.TransactionId));
        }

        private void RecoverBingoDisplay(
            CentralTransaction transaction,
            bool initialBallCall,
            bool isRecovery)
        {
            if (transaction?.Descriptions is null)
            {
                return;
            }

            foreach (var bingoGame in transaction.Descriptions.OfType<BingoGameDescription>())
            {
                var isDaubed = _unitOfWork.Invoke(
                    x => x.Repository<BingoDaubsModel>().Queryable()
                        .SingleOrDefault(y => y.GameIndex == bingoGame.GameIndex))?.CardIsDaubed ?? true;

                RecoverBingoCards(bingoGame);
                RecoverBallCall(initialBallCall, isDaubed, isRecovery, bingoGame);
                RecoverPatterns(isDaubed, isRecovery, bingoGame);
            }

            // Recover GEW message, if any
            if (isRecovery)
            {
                RecoverGameEndWinMessage(transaction);
            }
        }

        private void RecoverPatterns(bool showDaubs, bool isRecovery, BingoGameDescription bingoGame)
        {
            if (!showDaubs)
            {
                return;
            }

            var bingoPatterns = bingoGame.Patterns.ToList();
            Logger.Debug($"Recovering the bingo patterns: {string.Join(Environment.NewLine, bingoPatterns)}");
            _bus.Publish(new BingoGamePatternEvent(bingoPatterns, !_history.IsRecoveryNeeded && isRecovery, bingoGame.GameIndex));
        }

        private void RecoverBallCall(bool initialBallCall, bool showDaubs, bool isRecovery, BingoGameDescription bingoGame)
        {
            var cardDaubs = showDaubs ? GetDaubs(bingoGame.Cards.First(), initialBallCall) : 0;
            var bingoNumbers = (initialBallCall ? bingoGame.GetJoiningBalls() : bingoGame.BallCallNumbers).ToList();
            Logger.Debug($"Recovering the ball call: {string.Join(", ", bingoNumbers)}");
            _bus.Publish(new BingoGameBallCallEvent(new BingoBallCall(bingoNumbers), cardDaubs, isRecovery, bingoGame.GameIndex));
        }

        private void RecoverBingoCards(BingoGameDescription bingoGame)
        {
            foreach (var card in bingoGame.Cards)
            {
                Logger.Debug($"Recovering the bingo card: {card}");
                _bus.Publish(new BingoGameNewCardEvent(card, bingoGame.GameIndex));
            }
        }

        private async Task RecoveryBingoGamePlay(CentralTransaction transaction, CancellationToken token)
        {
            if (transaction?.OutcomeState == OutcomeState.Acknowledged ||
                transaction?.Descriptions?.FirstOrDefault() is not BingoGameDescription bingoGame)
            {
                return;
            }

            var log = GetGameHistory(transaction);
            var machineSerial = _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            if (log is not null && !bingoGame.GameEndWinClaimAccepted && bingoGame.Patterns.Any(p => p.IsGameEndWin))
            {
                try
                {
                    _gamePlayState.SetGameEndHold(true);
                    var strategy =
                        _unitOfWork.Invoke(
                            x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault()
                                ?.GameEndingPrize) ??
                        GameEndWinStrategy.Unknown;
                    var result = await (_gewFactory.Create(strategy)?.Recover(log.TransactionId, token) ??
                                        Task.FromResult(false));
                    bingoGame.GameEndWinClaimAccepted = result;
                    _centralProvider.UpdateOutcomeDescription(transaction.TransactionId, transaction.Descriptions);
                    Logger.Debug($"Recovered game end win result={result}");
                }
                finally
                {
                    _gamePlayState.SetGameEndHold(false);
                }
            }

            await _commandFactory.Execute(new BingoGameEndedCommand(machineSerial, transaction, log), token);
        }

        private void RecoverGameEndWinMessage(ITransactionConnector transaction)
        {
            var playedGame = GetGameHistory(transaction);
            if (playedGame is null || playedGame.GameWinBonus == 0)
            {
                return;
            }

            var gewBonus = _bonusHandler.Transactions.FirstOrDefault(
                t => t.Mode is BonusMode.GameWin && t.AssociatedTransactions.Contains(playedGame.TransactionId));
            if (gewBonus is null || gewBonus.DisplayMessageId == Guid.Empty)
            {
                return;
            }

            var gameEndWinMessage = Localizer.For(CultureFor.Player).FormatString(ResourceKeys.GameEndWinAward,
                playedGame.GameWinBonus.CentsToDollars().FormattedCurrencyString());
            _recoveredTransactionId = gewBonus.TransactionId;

            Logger.Debug($"Recover GEW: '{gameEndWinMessage}'");
            _recoveryMessageId = gewBonus.DisplayMessageId;
            var displayMessage = new DisplayableMessage(
                () => gameEndWinMessage,
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                typeof(BonusAwardedEvent),
                gewBonus.DisplayMessageId);

            _messages.DisplayMessage(displayMessage);
        }

        private void ClearGameEndWinMessage()
        {
            if (_recoveredTransactionId == 0)
            {
                return;
            }

            var gewBonus = _bonusHandler.Transactions.FirstOrDefault(
                t => t.Mode is BonusMode.GameWin && t.TransactionId == _recoveredTransactionId);
            if (gewBonus is null)
            {
                return;
            }

            gewBonus.DisplayMessageId = Guid.Empty;
            _transactionHistory.UpdateTransaction(gewBonus);

            _messages.RemoveMessage(_recoveryMessageId);
            _recoveredTransactionId = 0;
        }

        private void Handle(BankBalanceChangedEvent evt)
        {
            if (evt.NewBalance == 0)
            {
                ClearGameEndWinMessage();
            }
        }
    }
}