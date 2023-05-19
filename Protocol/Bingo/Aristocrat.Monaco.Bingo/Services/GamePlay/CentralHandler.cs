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
    using GameOutcome = Aristocrat.Bingo.Client.Messages.GamePlay.GameOutcome;

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
        private IEnumerable<int> _activeGames;

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

        private List<BingoGameDescription> GameDescription =>
            CurrentTransaction?.Descriptions?.OfType<BingoGameDescription>().ToList() ?? new List<BingoGameDescription>();

        /// <inheritdoc/>
        public Task<bool> ProcessGameOutcomes(GameOutcomes outcomes, CancellationToken token)
        {
            if (outcomes == null)
            {
                throw new ArgumentNullException(nameof(outcomes));
            }

            return ProcessGameOutcomesInternal(outcomes, token);
        }

        /// <inheritdoc/>
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
                throw new BingoGamePlayException("Got a Claim Game End Win when Game is inactive");
            }

            BingoPattern gewPattern;
            lock (_lock)
            {
                var description = GameDescription.FirstOrDefault(d => d.Cards.Any(x => x.IsGameEndWin));
                gewPattern = description?.Patterns?.FirstOrDefault(x => x.IsGameEndWin);

                if (gewPattern is null)
                {
                    throw new BingoGamePlayException("Game End Win pattern is not found");
                }
            }

            return ProcessClaimWinInternal(gewPattern, _currentGameTransactionId, token);
        }

        /// <inheritdoc />
        public Task RequestOutcomes(CentralTransaction transaction, bool isRecovering = false)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            return RequestOutcomesInternal(transaction);
        }

        /// <inheritdoc/>
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
            outcome.GameDetails.GameTitleId,
            0,
            OutcomeReference.Direct,
            OutcomeType.Standard,
            0,
            0,
            string.Empty,
            outcome.GameId,
            outcome.GameIndex);

        private static bool ShouldHandleWatTransferCompleted(WatTransferCommittedEvent evt)
        {
            var transferredAmount = evt.Transaction.TransferredCashableAmount +
                                    evt.Transaction.TransferredNonCashAmount +
                                    evt.Transaction.TransferredPromoAmount;
            return transferredAmount > 0;
        }

        private static IEnumerable<WinResult> GetOrderedWinResults(GameOutcomeWinDetails windDetails)
        {
            return windDetails.WinResults.OrderBy(x => x, DefaultWinResultComparer);
        }

        private async Task RequestOutcomesInternal(CentralTransaction transaction)
        {
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
                }).ConfigureAwait(false);

            var gamePlayTask = _currentGamePlayTask = HandleOutcomeRequest(transaction);
            await gamePlayTask.ConfigureAwait(false);
        }

        private async Task<bool> ProcessGameOutcomesInternal(GameOutcomes outcomes, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                if (outcomes.Outcomes.All(x => !x.IsSuccessful) || !_gameState.InGameRound)
                {
                    CheckForOutcomeResponseFailure();
                    return false;
                }

                await HandleBingoOutcomes(outcomes, token).ConfigureAwait(false);
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

        private async Task<bool> ProcessClaimWinInternal(BingoPattern gewPattern, long transactionId, CancellationToken token)
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
                    foreach (var description in GameDescription.Where(description => description.Cards.Any(g => g.IsGameEndWin)))
                    {
                        description.GameEndWinClaimAccepted = accepted;
                    }

                    _centralProvider.UpdateOutcomeDescription(
                        transactionId,
                        GameDescription);
                    return accepted;
                }
            }
            finally
            {
                _gewCancellationTokenSource = null;
            }
        }

        private async Task HandleBingoOutcomes(GameOutcomes gameOutcomes, CancellationToken token)
        {
            var outcomeTasks = new List<Task>();

            lock (_lock)
            {
                var descriptions = GameDescription;

                // Make sure we have a description for every game index
                foreach (var outcome in gameOutcomes.Outcomes)
                {
                    if (descriptions.All(x => x.GameIndex != outcome.GameIndex))
                    {
                        descriptions.Add(new BingoGameDescription { GameIndex = outcome.GameIndex });
                    }
                }

                // Feed the card overlay. We only care about new card data in the outcome response.
                var newCard = ProcessBingoCards(gameOutcomes, descriptions);
                var outcomes = new List<Outcome>();
                var setWaitingForPlayersTaskResult = true;
                var processBingoOutcomes = false;

                foreach (var gameOutcome in gameOutcomes.Outcomes)
                {
                    var description = descriptions.First(x => x.GameIndex == gameOutcome.GameIndex);
                    if (gameOutcome.WinDetails.WinResults.Any() || newCard)
                    {
                        AddProcessWinTasks(gameOutcome, description, outcomes, outcomeTasks, token);
                    }

                    token.ThrowIfCancellationRequested();

                    // When waiting for players, the server sends a game outcome with an empty ball call.
                    // TODO: this seems wrong - if not any balls then there shouldn't be any patterns
                    // TODO: could just send an empty list of BingoPatterns if we need this to clear the overlay
                    if (!gameOutcome.BingoDetails.BallCall.Any())
                    {
                        _eventBus.Publish(new BingoGamePatternEvent(description.Patterns.ToList(), false, gameOutcome.GameIndex));
                    }
                    else
                    {
                        // Attempt to set the task result once and use the return status to determine if
                        // the bingo outcomes should be processed.
                        if (setWaitingForPlayersTaskResult)
                        {
                            setWaitingForPlayersTaskResult = false;
                            processBingoOutcomes = _waitingForPlayersTask?.TrySetResult(true) ?? false;

                            if (processBingoOutcomes)
                            {
                                _eventBus.Publish(new PlayersFoundEvent());
                            }
                        }

                        if (processBingoOutcomes)
                        {
                            ProcessBingoOutcome(gameOutcome, description, outcomes);
                        }
                    }

                    AddBalls(gameOutcomes, descriptions);
                }

                UpdateCentralTransaction(outcomes, newCard, processBingoOutcomes, descriptions);
            }

            await Task.WhenAll(outcomeTasks).ConfigureAwait(false);
        }

        private void AddProcessWinTasks(
            GameOutcome gameOutcome,
            BingoGameDescription description,
            List<Outcome> processedOutcomes,
            List<Task> outcomeTasks,
            CancellationToken token)
        {
            var orderedPayout = GetOrderedWinResults(gameOutcome.WinDetails);
            outcomeTasks.AddRange(
                orderedPayout.Select(
                    winResult => ProcessWins(gameOutcome, winResult, processedOutcomes, description, token)));

            // Only the main game will use allow combined outcomes event. Side bet games are always assumed to be true.
            if (processedOutcomes.Any() && gameOutcome.GameIndex == 0)
            {
                _eventBus.Publish(new AllowCombinedOutcomesEvent(AllowCombinedOutcomes(processedOutcomes)));
            }
        }

        private void ProcessBingoOutcome(GameOutcome gameOutcome, BingoGameDescription description, List<Outcome> processedOutcomes)
        {
            description.JoinTime = DateTime.UtcNow;
            description.GameSerial = gameOutcome.GameDetails.GameSerial;
            description.GameTitleId = gameOutcome.GameDetails.GameTitleId;
            description.ThemeId = gameOutcome.GameDetails.ThemeId;
            description.DenominationId = gameOutcome.GameDetails.DenominationId;
            description.Paytable = gameOutcome.GameDetails.Paytable;
            description.GameEndWinEligibility = gameOutcome.BingoDetails.GameEndWinEligibility;

            var gameHasAnyOutcomes = processedOutcomes.FirstOrDefault(x => x.GameId == gameOutcome.GameId && x.GameIndex == gameOutcome.GameIndex) != null;
            if (!gameHasAnyOutcomes)
            {
                // We need to send an outcome otherwise we can't recover correctly
                processedOutcomes.Add(GetLosingGameOutcome(gameOutcome));
            }

            foreach (var card in description.Cards)
            {
                card.InitialDaubedBits = card.DaubedBits;
            }
        }

        private void UpdateCentralTransaction(
            IReadOnlyCollection<Outcome> outcomes,
            bool newCard,
            bool processOutcomes,
            IEnumerable<BingoGameDescription> gameDescriptions)
        {
            var bingoGameDescriptions = gameDescriptions.ToList();
            Logger.Debug($"UpdateCentralTransaction, outcomes count ={outcomes.Count}, newCard={newCard}, processOutcomes={processOutcomes}, gameDescriptions.Count={bingoGameDescriptions.Count}");

            // Send bingo pattern events for each bingo game description with wins
            foreach (var description in bingoGameDescriptions)
            {
                if (description.Patterns.Any())
                {
                    _eventBus.Publish(
                        new BingoGamePatternEvent(description.Patterns.ToList(), false, description.GameIndex));
                }
            }

            if ((outcomes.Any() || newCard) && processOutcomes)
            {
                _centralProvider.OutcomeResponse(
                    _currentGameTransactionId,
                    outcomes,
                    OutcomeException.None,
                    bingoGameDescriptions);
            }
            else
            {
                _centralProvider.UpdateOutcomeDescription(_currentGameTransactionId, bingoGameDescriptions);
            }
        }

        private bool ProcessBingoCards(
            GameOutcomes outcomes,
            IEnumerable<BingoGameDescription> descriptions)
        {
            var newCard = false;
            var gamesInPlay = new HashSet<int>();

            // for multi-game there will be 2 sets of cards, one for main game, and one for secondary game(s)
            // get the cards for each game
            foreach (var description in descriptions)
            {
                var cards = description.Cards.ToList();

                foreach (var outcome in outcomes.Outcomes.Where(x => x.GameIndex == description.GameIndex))
                {
                    gamesInPlay.Add(outcome.GameIndex);
                    foreach (var cardPlayed in outcome.BingoDetails.CardsPlayed)
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
                            _eventBus.Publish(new BingoGameNewCardEvent(card, outcome.GameIndex));
                        }
                        else
                        {
                            bingoCard.DaubedBits = cardPlayed.BitPattern;
                            bingoCard.IsGameEndWin = cardPlayed.IsGameEndWin;
                        }
                    }

                    if (!newCard)
                    {
                        continue;
                    }

                    description.Cards = cards;
                    description.GameEndWinEligibility = outcome.BingoDetails.GameEndWinEligibility;
                }
            }

            DisableCards(gamesInPlay);
            _activeGames = gamesInPlay;
            return newCard;
        }

        private void DisableCards(IEnumerable<int> gamesInPlay)
        {
            if (_activeGames is null)
            {
                return;
            }
            foreach (var gameIndex in _activeGames.Where(x => !gamesInPlay.Contains(x)))
            {
                Logger.Debug($"Disabling card for out-of-play game index {gameIndex}");
                _eventBus.Publish(new BingoGameDisableCardEvent(gameIndex));
            }
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
                var subGameId = currentGame?.SupportedSubGames?.FirstOrDefault()?.CdsTitleId ?? string.Empty;
                var requests = transaction.GenerateMultiPlayRequest(
                    machineSerial,
                    details,
                    currentGame.GetBingoTitleIdInt(),
                    int.TryParse(subGameId, out _) ? int.Parse(subGameId) : null); // TODO this needs to use ActiveSubGames once it is available
                await _commandFactory.Execute(
                    requests,
                    source.Token).ConfigureAwait(false);
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
                _waitingForPlayersTask?.TrySetCanceled(CancellationToken.None);
                _waitingForPlayersTask = new TaskCompletionSource<bool>();
                using var registration = token.Register(() => _waitingForPlayersTask?.TrySetCanceled());
                var waitingForPlayersTime =
                    _unitOfWorkFactory
                        .Invoke(x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault())
                        ?.WaitingForPlayersMs?.Milliseconds() ?? BingoConstants.DefaultWaitForPlayersSeconds.Seconds();
                var playersEvent = new WaitingForPlayersEvent(DateTime.UtcNow, waitingForPlayersTime);
                _eventBus.Publish(playersEvent);
                await _waitingForPlayersTask!.TimeoutAfter(playersEvent.WaitingDuration).ConfigureAwait(false);
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
                        outcome.GameDetails.GameSerial,
                        pattern.CardSerial,
                        CurrentTransaction.WagerAmount.MillicentsToCents()),
                    token).ConfigureAwait(false);
            }
            // Otherwise it's a standard win.
            else
            {
                outcomes.Add(
                    new Outcome(
                        DateTime.UtcNow.Ticks,
                        outcome.GameDetails.GameTitleId,
                        winResult.PaytableId,
                        OutcomeReference.Direct,
                        OutcomeType.Standard,
                        winAmount.CentsToMillicents(),
                        winResult.WinIndex,
                        winResult.PatternName,
                        outcome.GameId,
                        outcome.GameIndex));

                Logger.Debug($"New pattern:{pattern}, win:{winAmount}");
            }
        }

        private void EndCurrentGame()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void AddBalls(GameOutcomes outcomes, IEnumerable<BingoGameDescription> descriptions)
        {
            var balls = outcomes.Outcomes.First().BingoDetails.BallCall.ToList();
            if (!balls.Any())
            {
                foreach (var description in descriptions)
                {
                    description.JoinBallIndex = -1;
                }

                return;
            }

            List<BingoNumber> bingoNumbers = new();
            for (var index = 0; index < balls.Count; index++)
            {
                var state = index >= BingoConstants.InitialBallDraw
                    ? BingoNumberState.BallCallLate
                    : BingoNumberState.BallCallInitial;
                bingoNumbers.Add(new BingoNumber(balls[index], state));
            }

            var ballCall = new BingoBallCall(bingoNumbers);

            foreach (var description in descriptions)
            {
                description.BallCallNumbers = bingoNumbers;
                if (description.JoinBallIndex < 0)
                {
                    description.JoinBallIndex = outcomes.Outcomes.First().BingoDetails.JoinBallNumber;
                }
            }

            foreach (var outcome in outcomes.Outcomes)
            {
                _eventBus.Publish(
                new BingoGameBallCallEvent(ballCall, outcome.BingoDetails.CardsPlayed.First().BitPattern, false, outcome.GameIndex));
            }
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