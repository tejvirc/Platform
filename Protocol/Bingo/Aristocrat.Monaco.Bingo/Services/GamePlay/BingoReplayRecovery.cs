namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
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
    using Monaco.Common;
    using Protocol.Common.Storage.Entity;
    using GameEndWinFactory =
        Common.IBingoStrategyFactory<GameEndWin.IGameEndWinStrategy, Common.Storage.Model.GameEndWinStrategy>;

    public class BingoReplayRecovery : IBingoReplayRecovery, IDisposable
    {
        private static readonly Guid RecoveryGameEndWinMessageKey = new("{13fc85f5-8146-4049-8589-83ff711607a0}");
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _bus;
        private readonly ICentralProvider _centralProvider;
        private readonly IPropertiesManager _properties;
        private readonly IGameHistory _history;
        private readonly IMessageDisplay _messages;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IUnitOfWorkFactory _unitOfWork;
        private readonly GameEndWinFactory _gewFactory;
        private readonly ManualResetEvent _gameLoaded = new(false);

        private bool _disposed;
        private string _recoveredGameEndWinMessage;

        public BingoReplayRecovery(
            IEventBus bus,
            ICentralProvider centralProvider,
            IPropertiesManager properties,
            IGameHistory history,
            IMessageDisplay messages,
            ICommandHandlerFactory commandFactory,
            IUnitOfWorkFactory unitOfWork,
            GameEndWinFactory gewFactory)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _messages = messages ?? throw new ArgumentNullException(nameof(messages));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _gewFactory = gewFactory ?? throw new ArgumentNullException(nameof(gewFactory));

            _bus.Subscribe<GameLoadedEvent>(this, _ => _gameLoaded.Set());
            _bus.Subscribe<GameProcessExitedEvent>(this, _ => _gameLoaded.Reset());
            _bus.Subscribe<GamePlayInitiatedEvent>(this, _ => ClearGameEndWinMessage());
        }

        public TimeSpan GameLoadedWaitInterval { get; set; } = TimeSpan.FromMilliseconds(10000);

        public async Task RecoverDisplay(CancellationToken token)
        {
            await RecoverBingoDisplay(GetLastTransaction(), false, true, token);
        }

        public async Task RecoverGamePlay(CancellationToken token)
        {
            await RecoveryBingoGamePlay(GetLastTransaction(), token);
        }

        public async Task Replay(IGameHistoryLog log, bool finalizeReplay, CancellationToken token)
        {
            var transaction = _centralProvider.Transactions.FirstOrDefault(x => x.AssociatedTransactions.Contains(log.TransactionId));
            await RecoverBingoDisplay(transaction, !finalizeReplay, false, token);
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
                _gameLoaded.Dispose();
            }

            _disposed = true;
        }

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

        private async Task RecoverBingoDisplay(
            CentralTransaction transaction,
            bool initialBallCall,
            bool isRecovery,
            CancellationToken token)
        {
            if (transaction?.Descriptions?.FirstOrDefault() is not BingoGameDescription bingoGame)
            {
                return;
            }

            await _gameLoaded.AsTask(GameLoadedWaitInterval, token);
            foreach (var card in bingoGame.Cards)
            {
                Logger.Debug($"Recovering the bingo card: {card}");
                _bus.Publish(new BingoGameNewCardEvent(card));
            }

            var bingoNumbers = (initialBallCall ? bingoGame.GetJoiningBalls() : bingoGame.BallCallNumbers).ToList();
            Logger.Debug($"Recovering the ball call: {string.Join(", ", bingoNumbers)}");
            _bus.Publish(new BingoGameBallCallEvent(new BingoBallCall(bingoNumbers), bingoGame.Cards.First().DaubedBits));

            var bingoPatterns = bingoGame.Patterns.ToList();
            Logger.Debug($"Recovering the bingo patterns: {string.Join(Environment.NewLine, bingoPatterns)}");
            _bus.Publish(new BingoGamePatternEvent(bingoPatterns, !_history.IsRecoveryNeeded));

            // Recover GEW message, if any
            if (isRecovery)
            {
                RecoverGameEndWinMessage(transaction);
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
                var strategy =
                    _unitOfWork.Invoke(
                        x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault()?.GameEndingPrize) ??
                    GameEndWinStrategy.Unknown;
                var result = await (_gewFactory.Create(strategy)?.Recover(log.TransactionId, token) ?? Task.FromResult(false));
                bingoGame.GameEndWinClaimAccepted = result;
                _centralProvider.UpdateOutcomeDescription(transaction.TransactionId, transaction.Descriptions);
                Logger.Debug($"Recovered game end win result={result}");
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

            _recoveredGameEndWinMessage = Localizer.For(CultureFor.Player).FormatString(ResourceKeys.GameEndWinAward,
                playedGame.GameWinBonus.CentsToDollars().FormattedCurrencyString());

            Logger.Debug($"Recover GEW: '{_recoveredGameEndWinMessage}'");

            var displayMessage = new DisplayableMessage(
                GetGameEndWinMessage,
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                typeof(BonusAwardedEvent),
                RecoveryGameEndWinMessageKey);

            _messages.DisplayMessage(displayMessage);
        }

        private void ClearGameEndWinMessage()
        {
            _recoveredGameEndWinMessage = "";
            _messages.RemoveMessage(RecoveryGameEndWinMessageKey);
        }

        private string GetGameEndWinMessage()
        {
            return _recoveredGameEndWinMessage;
        }
    }
}