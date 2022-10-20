namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Commands;
    using Common;
    using Common.Events;
    using Common.Exceptions;
    using Common.Extensions;
    using Common.GameOverlay;
    using Common.Storage;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Humanizer;
    using Kernel;
    using log4net;
    using Monaco.Common;
    using Protocol.Common.Storage.Entity;
    using GameEndWinFactory =
        Common.IBingoStrategyFactory<GameEndWin.IGameEndWinStrategy, Common.Storage.Model.GameEndWinStrategy>;

    public class CentralHandler : ICentralHandler, IBingoGameOutcomeHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly IBetDetails DefaultBetDetails = new BetDetails(0, 0, 0, 0, 0);
        private static readonly IComparer<WinResult> DefaultWinResultComparer = new WinResultComparer();

        private static readonly IReadOnlyCollection<Guid> AllowedGameDisables = new[]
        {
            ApplicationConstants.HandpayPendingDisableKey, ApplicationConstants.LiveAuthenticationDisableKey
        };

        private readonly IEventBus _eventBus;
        private readonly IPlayerBank _bank;
        private readonly ICentralProvider _centralProvider;
        private readonly IGameProvider _gameProvider;
        private readonly IGamePlayState _gameState;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _systemDisable;
        private readonly IBingoCardProvider _cardProvider;
        private readonly GameEndWinFactory _gewStrategyFactory;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly object _lock = new();

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _gewCancellationTokenSource;
        private long _currentGameTransactionId;
        private bool _disposed;
        private Task _currentGamePlayTask;
        private TaskCompletionSource<bool> _waitingForPlayersTask;
        private long _lastDenom;
        private BetDetails _lastBetDetail;

        public CentralHandler(
            IEventBus eventBus,
            IPlayerBank bank,
            ICentralProvider centralProvider,
            IGameProvider gameProvider,
            IGamePlayState gameState,
            IPropertiesManager properties,
            ISystemDisableManager systemDisable,
            IBingoCardProvider cardProvider,
            GameEndWinFactory gewStrategyFactory,
            ICommandHandlerFactory commandFactory,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _cardProvider = cardProvider ?? throw new ArgumentNullException(nameof(cardProvider));
            _gewStrategyFactory = gewStrategyFactory ?? throw new ArgumentNullException(nameof(gewStrategyFactory));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

            _centralProvider.Register(this, ProtocolNames.Bingo);

            SubscribeToEvents();
        }

        private CentralTransaction CurrentTransaction =>
            _centralProvider.Transactions.FirstOrDefault(x => x.TransactionId == _currentGameTransactionId);

        private BingoGameDescription GameDescription =>
            CurrentTransaction?.Descriptions?.FirstOrDefault() as BingoGameDescription ?? new BingoGameDescription();

        public async Task<bool> ProcessGameOutcome(GameOutcome outcome, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                if (!outcome.IsSuccessful || !_gameState.InGameRound)
                {
                    CheckForOutcomeResponseFailure();
                    return false;
                }

                await HandleBingoOutcomes(outcome, token);
            }
            catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
            {
                Logger.Warn("Game outcome cancelled", e);
                return false;
            }
            catch (Exception e)
            {
                Logger.Error("Game outcome format error", e);
                CheckForOutcomeResponseFailure();
                return false;
            }

            return true;
        }

        public Task<bool> ProcessClaimWin(ClaimWinResults claim, CancellationToken token)
        {
            Logger.Debug($"Received a claim win response with Accepted={claim.Accepted}");
            token.ThrowIfCancellationRequested();

            if (!claim.Accepted)
            {
                return Task.FromResult(false);
            }

            if (!_gameState.InGameRound)
            {
                throw new BingoGamePlayException("Game is inactive");
            }

            var transactionId = _currentGameTransactionId;
            BingoPattern gewPattern;
            lock (_lock)
            {
                var description = GameDescription;
                gewPattern = description.Patterns.FirstOrDefault(x => x.IsGameEndWin);

                if (gewPattern is null)
                {
                    throw new BingoGamePlayException("Game End Win pattern is not found");
                }

                Logger.Debug("Game is active, provide claim win");
            }

            return HandleClaimResults(gewPattern, transactionId, token);
        }

        /// <inheritdoc />
        public async Task RequestOutcomes(CentralTransaction transaction, bool isRecovering = false)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            _cancellationTokenSource?.Cancel();
            _gewCancellationTokenSource?.Cancel();
            var lastGame = _currentGamePlayTask ?? Task.CompletedTask;
            await lastGame.ContinueWith(
                t =>
                {
                    if (!t.IsFaulted)
                    {
                        return;
                    }

                    Logger.Warn("An error occurred on the previous game", t.Exception);
                    t.Exception?.Handle(_ => true);
                });

            var task = _currentGamePlayTask = HandleOutcomeRequest(transaction);
            await task.ConfigureAwait(false);
        }

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
                _eventBus.UnsubscribeAll(this);
                _centralProvider.Clear(ProtocolNames.Bingo);
                _waitingForPlayersTask?.TrySetCanceled();

                _cancellationTokenSource?.Cancel(true);
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }

                _gewCancellationTokenSource?.Cancel(true);
                if (_gewCancellationTokenSource != null)
                {
                    _gewCancellationTokenSource.Dispose();
                    _gewCancellationTokenSource = null;
                }
            }

            _disposed = true;
        }

        private static Outcome GetLosingGameOutcome(GameOutcome outcome) => new(
            DateTime.UtcNow.Ticks,
            outcome.GameTitleId,
            0,
            OutcomeReference.Direct,
            OutcomeType.Standard,
            0,
            0,
            string.Empty);

        private static bool ShouldHandleWatTransferCompleted(WatTransferCommittedEvent evt)
        {
            var transferredAmount = evt.Transaction.TransferredCashableAmount +
                                    evt.Transaction.TransferredNonCashAmount +
                                    evt.Transaction.TransferredPromoAmount;
            return transferredAmount > 0;
        }

        private static IEnumerable<WinResult> GetOrderedWinResults(GameOutcome outcome)
        {
            return outcome.WinResults.OrderBy(x => x, DefaultWinResultComparer);
        }

        private async Task<bool> HandleClaimResults(BingoPattern gewPattern, long transactionId, CancellationToken token)
        {
            using var source = new CancellationTokenSource();
            _gewCancellationTokenSource = source;

            try
            {
                var serverSettingsModel = _unitOfWorkFactory.Invoke(
                    x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault());
                var winStrategy = serverSettingsModel?.GameEndingPrize ?? GameEndWinStrategy.BonusCredits;
                var gameEndWinStrategy = _gewStrategyFactory.Create(winStrategy);
                var winAmount = gewPattern.WinAmount.CentsToMillicents();
                token.ThrowIfCancellationRequested();
                var accepted = await gameEndWinStrategy.ProcessWin(winAmount, source.Token).ConfigureAwait(false);

                lock (_lock)
                {
                    var description = GameDescription ?? new BingoGameDescription();
                    description.GameEndWinClaimAccepted = accepted;
                    _centralProvider.UpdateOutcomeDescription(
                        transactionId,
                        new List<IOutcomeDescription> { description });
                    return description.GameEndWinClaimAccepted;
                }
            }
            finally
            {
                _gewCancellationTokenSource = null;
            }
        }

        private async Task HandleBingoOutcomes(GameOutcome outcome, CancellationToken token)
        {
            var outcomeTasks = new List<Task>();

            lock (_lock)
            {
                var description = GameDescription;

                // Feed the card overlay. We only care about new card data in the outcome response.
                var newCard = ProcessBingoCards(outcome, description);
                var outcomes = new List<Outcome>();
                if (outcome.WinResults.Any() || newCard)
                {
                    var orderedPayOut = GetOrderedWinResults(outcome);
                    outcomeTasks.AddRange(
                        orderedPayOut.Select(
                            winResult => ProcessWins(outcome, winResult, outcomes, description, token)));
                    if (outcomes.Any())
                    {
                        _eventBus.Publish(new AllowCombinedOutcomesEvent(AllowCombinedOutcomes(outcomes)));
                    }
                }

                token.ThrowIfCancellationRequested();

                // When waiting for players, the server sends a game outcome with an empty ball call.
                if (!outcome.BallCall.Any())
                {
                    _eventBus.Publish(new BingoGamePatternEvent(description.Patterns.ToList()));
                }
                else
                {
                    if (_waitingForPlayersTask?.TrySetResult(true) ?? false)
                    {
                        description.JoinTime = DateTime.UtcNow;
                        description.GameSerial = outcome.GameSerial;
                        description.GameTitleId = outcome.GameTitleId;
                        description.ThemeId = outcome.ThemeId;
                        description.DenominationId = outcome.DenominationId;
                        description.Paytable = outcome.Paytable;
                        description.GameEndWinEligibility = outcome.GameEndWinEligibility;
                        _eventBus.Publish(new PlayersFoundEvent());
                        if (!outcomes.Any())
                        {
                            // We need to send an outcome otherwise we can't recover correctly
                            outcomes.Add(GetLosingGameOutcome(outcome));
                        }

                        foreach (var card in description.Cards)
                        {
                            card.InitialDaubedBits = card.DaubedBits;
                        }
                    }

                    AddBalls(outcome, description);
                    UpdateCentralTransaction(outcomes, newCard, description);
                }
            }

            await Task.WhenAll(outcomeTasks);
        }

        private void UpdateCentralTransaction(
            IReadOnlyCollection<Outcome> outcomes,
            bool newCard,
            BingoGameDescription description)
        {
            var descriptions = new List<IOutcomeDescription> { description };
            if (outcomes.Any() || newCard)
            {
                _centralProvider.OutcomeResponse(
                    _currentGameTransactionId,
                    outcomes,
                    OutcomeException.None,
                    descriptions);
                _eventBus.Publish(new BingoGamePatternEvent(description.Patterns.ToList()));
            }
            else
            {
                _centralProvider.UpdateOutcomeDescription(_currentGameTransactionId, descriptions);
            }
        }

        private bool ProcessBingoCards(GameOutcome outcome, BingoGameDescription description)
        {
            var newCard = false;
            var cards = description.Cards.ToList();
            foreach (var cardPlayed in outcome.CardsPlayed)
            {
                var bingoCard = cards.FirstOrDefault(c => c.SerialNumber == cardPlayed.SerialNumber);
                if (bingoCard is null)
                {
                    var card = _cardProvider.GetCardBySerial(cardPlayed.SerialNumber);
                    card.DaubedBits = cardPlayed.BitPattern;
                    card.IsGameEndWin = cardPlayed.IsGameEndWin;
                    newCard = true;
                    Logger.Debug($"New card: {card}");
                    cards.Add(card);
                    _eventBus.Publish(new BingoGameNewCardEvent(card));
                }
                else
                {
                    bingoCard.DaubedBits = cardPlayed.BitPattern;
                }
            }

            if (newCard)
            {
                description.Cards = cards;
            }

            return newCard;
        }

        private async Task HandleOutcomeRequest(CentralTransaction transaction)
        {
            using var source = new CancellationTokenSource();
            _cancellationTokenSource = source;

            try
            {
                Logger.Info($"Requesting outcomes for {transaction}");

                _gameState.SetGameEndHold(true);
                _currentGameTransactionId = transaction.TransactionId;

                var currentGame = _gameProvider.GetGame(transaction.GameId);

                var machineSerial = _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty);
                var details = _properties.GetValue(
                    GamingConstants.SelectedBetDetails,
                    DefaultBetDetails);

                WaitingForPlayers(transaction, source.Token).FireAndForget();
                await _commandFactory.Execute(
                    new RequestPlayCommand(
                        machineSerial,
                        transaction.WagerAmount.MillicentsToCents(),
                        (int)transaction.Denomination.MillicentsToCents(),
                        details.BetLinePresetId,
                        details.BetPerLine,
                        details.NumberLines,
                        details.Ante,
                        currentGame.GetBingoTitleId()),
                    source.Token);
            }
            catch (Exception ex)
            {
                Logger.Error("RequestOutcomes failed", ex);
                if (CheckForOutcomeResponseFailure(transaction))
                {
                    _eventBus.Publish(new WaitingForPlayersCanceledEvent());
                }
            }
            finally
            {
                if (CheckForOutcomeResponseFailure(transaction, OutcomeException.TimedOut))
                {
                    _eventBus.Publish(new NoPlayersFoundEvent());
                }

                _gameState.SetGameEndHold(false);
                _cancellationTokenSource = null;
            }
        }

        private async Task WaitingForPlayers(ITransaction centralTransaction, CancellationToken token)
        {
            try
            {
                _waitingForPlayersTask?.TrySetCanceled();
                _waitingForPlayersTask = new TaskCompletionSource<bool>();
                using var registration = token.Register(() => _waitingForPlayersTask?.TrySetCanceled());
                var waitingForPlayersTime =
                    _unitOfWorkFactory
                        .Invoke(x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault())
                        ?.WaitingForPlayersMs?.Milliseconds() ?? BingoConstants.DefaultWaitForPlayersSeconds.Seconds();
                var playersEvent = new WaitingForPlayersEvent(DateTime.UtcNow, waitingForPlayersTime);
                _eventBus.Publish(playersEvent);
                await _waitingForPlayersTask.TimeoutAfter(playersEvent.WaitingDuration);
            }
            catch (TimeoutException)
            {
                if (CheckForOutcomeResponseFailure(centralTransaction, OutcomeException.TimedOut))
                {
                    _eventBus.Publish(new NoPlayersFoundEvent());
                    _waitingForPlayersTask?.TrySetResult(false);
                }
            }
            finally
            {
                _waitingForPlayersTask = null;
            }
        }

        private void SubscribeToEvents()
        {
            // Events for ending the bingo game round, and GEW eligibility
            _eventBus.Subscribe<OutcomeFailedEvent>(this, _ => EndCurrentGame());
            _eventBus.Subscribe<SystemDisableAddedEvent>(this, HandleSystemDisabledAddedEvent);
            _eventBus.Subscribe<CashOutStartedEvent>(this, _ => EndCurrentGame(), evt => evt.ZeroRemaining);
            _eventBus.Subscribe<WatTransferCommittedEvent>(
                this,
                HandleWatTransferCompleted,
                ShouldHandleWatTransferCompleted);
            _eventBus.Subscribe<PropertyChangedEvent>(
                this,
                HandleGamePropertiesChanged,
                evt => evt.PropertyName is GamingConstants.SelectedDenom or GamingConstants.SelectedBetDetails);
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, _ => EndCurrentGame());
        }

        private void HandleSystemDisabledAddedEvent(SystemDisableAddedEvent evt)
        {
            if (!_systemDisable.CurrentImmediateDisableKeys.Except(AllowedGameDisables).Any())
            {
                return;
            }

            EndCurrentGame();
        }

        private void HandleWatTransferCompleted(WatTransferCommittedEvent evt)
        {
            if (_bank.Credits != 0)
            {
                return;
            }

            EndCurrentGame();
        }

        private void HandleGamePropertiesChanged(PropertyChangedEvent evt)
        {
            EndCurrentGame();
            var selectedDenom = _properties.GetValue(GamingConstants.SelectedDenom, 0L);
            var selectedBet = _properties.GetValue<BetDetails>(GamingConstants.SelectedBetDetails, null);
            var shouldClearDaubs = ShouldClearDaubs(selectedDenom, selectedBet);
            _lastDenom = selectedDenom;
            _lastBetDetail = selectedBet;
            if (!shouldClearDaubs)
            {
                return;
            }

            _eventBus.Publish(new ClearBingoDaubsEvent());
        }

        private bool ShouldClearDaubs(long selectedDenom, BetDetails selectedBet)
        {
            return _lastDenom != 0 &&
                   _lastBetDetail is not null &&
                   (selectedDenom != _lastDenom || !Equals(selectedBet, _lastBetDetail));
        }

        private bool CheckForOutcomeResponseFailure(
            ITransaction transaction = null,
            OutcomeException exception = OutcomeException.Invalid)
        {
            var transactionId = transaction?.TransactionId ?? _currentGameTransactionId;
            var pending = _centralProvider.Transactions.FirstOrDefault(x => x.TransactionId == transactionId)?
                .OutcomeState == OutcomeState.Requested;
            if (pending)
            {
                _centralProvider.OutcomeResponse(
                    transactionId,
                    new List<Outcome>(),
                    exception);
            }

            return pending;
        }

        private async Task ProcessWins(
            GameOutcome outcome,
            WinResult winResult,
            ICollection<Outcome> outcomes,
            BingoGameDescription description,
            CancellationToken token)
        {
            var pattern = winResult.ToBingoPattern();
            if (description.Patterns.Contains(pattern))
            {
                return;
            }

            description.Patterns = description.Patterns.Append(pattern).ToList();
            var winAmount = winResult.Payout;

            // Can we claim the GEW?
            if (winResult.IsGameEndWin)
            {
                await _commandFactory.Execute(
                    new ClaimWinCommand(
                        _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty),
                        outcome.GameSerial,
                        pattern.CardSerial,
                        CurrentTransaction.WagerAmount.MillicentsToCents()),
                    token);
            }
            // Otherwise it's a standard win.
            else
            {
                outcomes.Add(
                    new Outcome(
                        DateTime.UtcNow.Ticks,
                        outcome.GameTitleId,
                        winResult.PaytableId,
                        OutcomeReference.Direct,
                        OutcomeType.Standard,
                        winAmount.CentsToMillicents(),
                        winResult.WinIndex,
                        winResult.PatternName));

                Logger.Debug($"New pattern:{pattern}, win:{winAmount}");
            }
        }

        private void EndCurrentGame()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void AddBalls(GameOutcome outcome, BingoGameDescription description)
        {
            var balls = outcome.BallCall.ToList();
            if (!balls.Any())
            {
                description.JoinBallIndex = -1;
                return;
            }

            List<BingoNumber> bingoNumbers = new();
            for (var index = 0; index < balls.Count; index++)
            {
                var state = index >= BingoConstants.InitialBallDraw ? BingoNumberState.BallCallLate : BingoNumberState.BallCallInitial;
                bingoNumbers.Add(new BingoNumber(balls[index], state));
            }

            var ballCall = new BingoBallCall(bingoNumbers);
            description.BallCallNumbers = bingoNumbers;
            if (description.JoinBallIndex < 0)
            {
                description.JoinBallIndex = outcome.JoinBallNumber;
            }

            _eventBus.Publish(new BingoGameBallCallEvent(ballCall, outcome.CardsPlayed.First().BitPattern));
        }

        private bool AllowCombinedOutcomes(IReadOnlyCollection<Outcome> outcomes)
        {
            var determination = _unitOfWorkFactory.Invoke(
                x => x.Repository<BingoServerSettingsModel>().Queryable().Single().JackpotAmountDetermination);
            var largeWinLimit = _properties.GetValue(
                AccountingConstants.LargeWinLimit,
                AccountingConstants.DefaultLargeWinLimit);
            return determination == JackpotDetermination.InterimPattern && outcomes.Max(o => o.Value) < largeWinLimit ||
                   determination == JackpotDetermination.TotalWins && outcomes.Sum(o => o.Value) < largeWinLimit;
        }
    }
}