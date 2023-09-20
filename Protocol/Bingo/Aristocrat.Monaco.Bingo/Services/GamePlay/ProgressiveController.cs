namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Bingo.Client.Messages;
    using Common;
    using Common.Events;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using log4net;
    using Newtonsoft.Json;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     Implements <see cref="IProgressiveController" />.
    /// </summary>
    public sealed class ProgressiveController : IProgressiveController, IDisposable, IProtocolProgressiveEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IProtocolProgressiveEventsRegistry _multiProtocolEventBusRegistry;
        private readonly IGameHistory _gameHistory;
        private readonly IProgressiveContributionService _progressiveContributionService;
        private readonly IProgressiveClaimService _progressiveClaimService;
        private readonly IProgressiveAwardService _progressiveAwardService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IBingoGameOutcomeHandler _bingoGameOutcomeHandler;

        private readonly ConcurrentDictionary<string, IList<ProgressiveInfo>> _progressives = new();
        private readonly IList<ProgressiveInfo> _activeProgressiveInfos = new List<ProgressiveInfo>();
        private readonly SemaphoreSlim _pendingAwardsLock = new(1);
        private IList<(string poolName, long progressiveLevelId, long amountInPennies, int awardId)> _pendingAwards = new List<(string, long, long, int)>();
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveController" /> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus" />.</param>
        /// <param name="gameProvider"><see cref="IGameProvider" />.</param>
        /// <param name="protocolLinkedProgressiveAdapter">.</param>
        /// <param name="gameHistory"><see cref="IGameHistory" />.</param>
        /// <param name="multiProtocolEventBusRegistry"><see cref="IProtocolProgressiveEventsRegistry" />.</param>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory" />.</param>
        /// <param name="progressiveContributionService"><see cref="IProgressiveContributionService" />.</param>
        /// <param name="progressiveClaimService"><see cref="IProgressiveClaimService" />.</param>
        /// <param name="progressiveAwardService"><see cref="IProgressiveAwardService" />.</param>
        /// <param name="propertiesManager"><see cref="IPropertiesManager" />.</param>
        /// <param name="bingoGameOutcomeHandler"><see cref="IBingoGameOutcomeHandler" />.</param>
        public ProgressiveController(
            IEventBus eventBus,
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IGameHistory gameHistory,
            IProtocolProgressiveEventsRegistry multiProtocolEventBusRegistry,
            IUnitOfWorkFactory unitOfWorkFactory,
            IProgressiveContributionService progressiveContributionService,
            IProgressiveClaimService progressiveClaimService,
            IProgressiveAwardService progressiveAwardService,
            IPropertiesManager propertiesManager,
            IBingoGameOutcomeHandler bingoGameOutcomeHandler)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _multiProtocolEventBusRegistry = multiProtocolEventBusRegistry ?? throw new ArgumentNullException(nameof(multiProtocolEventBusRegistry));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _progressiveContributionService = progressiveContributionService ?? throw new ArgumentNullException(nameof(progressiveContributionService));
            _progressiveClaimService = progressiveClaimService ?? throw new ArgumentNullException(nameof(progressiveClaimService));
            _progressiveAwardService = progressiveAwardService ?? throw new ArgumentNullException(nameof(progressiveAwardService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bingoGameOutcomeHandler = bingoGameOutcomeHandler ?? throw new ArgumentNullException(nameof(bingoGameOutcomeHandler));

            SubscribeToEvents();
        }

        /// <inheritdoc />
        public void HandleProgressiveEvent<T>(T @event)
        {
            _ = Handle(@event as LinkedProgressiveHitEvent, CancellationToken.None);
        }

        /// <inheritdoc />
        public async Task AwardJackpot(string poolName, long amountInPennies, CancellationToken token)
        {
            Logger.Debug($"AwardJackpot, poolName={poolName}, amountInPennies={amountInPennies}");

            if (_gameHistory?.CurrentLog.PlayState == PlayState.Idle)
            {
                return;
            }

            if (string.IsNullOrEmpty(poolName) || amountInPennies <= 0)
            {
                return;
            }

            IViewableProgressiveLevel progressiveLevel = null;
            foreach (var level in _protocolLinkedProgressiveAdapter.ViewConfiguredProgressiveLevels())
            {
                var levelPoolName = CreatePoolName(level.ProgressivePackName, level.LevelName, level.ResetValue);
                if (levelPoolName == poolName)
                {
                    progressiveLevel = level;
                    break;
                }
            }

            if (progressiveLevel is null)
            {
                throw new InvalidOperationException($"No progressive levels found matching '{poolName}'");
            }

            var levelId = progressiveLevel.LevelId;
            var progressiveId = progressiveLevel.ProgressiveId;
            await _pendingAwardsLock.WaitAsync(token);
            ProgressiveAwardRequestMessage request;
            try
            {
                var machineSerial = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);

                request = new ProgressiveAwardRequestMessage(
                    machineSerial,
                    progressiveId,
                    levelId,
                    amountInPennies,
                    true);
            }
            finally
            {
                _pendingAwardsLock.Release();
            }

            await _progressiveAwardService.AwardProgressive(request, token);
        }

        /// <inheritdoc />
        public void Configure()
        {
            Logger.Debug("ProgressiveController Configure");

            _progressives.Clear();
            _activeProgressiveInfos.Clear();

            // handle if there are pending awards
            _pendingAwardsLock.Wait();
            try
            {
                if (_pendingAwards is null)
                {
                    using var unitOfWork = _unitOfWorkFactory.Create();
                    var pendingJackpots = unitOfWork.Repository<PendingJackpotAwards>().Queryable().SingleOrDefault();
                    _pendingAwards = pendingJackpots?.Awards is null
                        ? new List<(string, long, long, int)>()
                        : JsonConvert.DeserializeObject<IList<(string, long, long, int)>>(pendingJackpots.Awards);
                }
            }
            finally
            {
                _pendingAwardsLock.Release();
            }

            var pools =
                (from level in _protocolLinkedProgressiveAdapter.ViewProgressiveLevels()
                        .Where(
                            x => x.LevelType == ProgressiveLevelType.LP)
                    group level by new
                    {
                        level.GameId,
                        PackName = level.ProgressivePackName,
                        ProgId = level.ProgressiveId,
                        level.LevelId,
                        level.LevelName
                    }
                    into pool
                    orderby pool.Key.GameId, pool.Key.PackName, pool.Key.ProgId, pool.Key.LevelId
                    select pool).ToArray();

            foreach (var pool in pools)
            {
                var game = _gameProvider.GetGame(pool.Key.GameId);
                if (game is null)
                {
                    continue;
                }

                var resetValue = pool.First().ResetValue;

                var poolName = CreatePoolName(pool.Key.PackName, pool.Key.LevelName, resetValue);

                var progressive = new ProgressiveInfo(
                    pool.Key.PackName,
                    pool.Key.ProgId,
                    pool.Key.LevelId,
                    pool.Key.LevelName,
                    poolName);

                if (!_progressives.ContainsKey(poolName))
                {
                    _progressives.TryAdd(poolName, new List<ProgressiveInfo>());
                }

                if (game.EgmEnabled &&
                    !_activeProgressiveInfos.Any(p => p.PoolName.Equals(progressive.PoolName, StringComparison.OrdinalIgnoreCase)))
                {
                    _activeProgressiveInfos.Add(progressive);
                }

                _progressives[poolName].Add(progressive);

                var linkedLevel = UpdateLinkedProgressiveLevels(
                    pool.Key.ProgId,
                    pool.Key.LevelId,
                    resetValue.MillicentsToCents(),
                    true);

                _protocolLinkedProgressiveAdapter.AssignLevelsToGame(
                    pool.Select(
                        level => new ProgressiveLevelAssignment(
                        game,
                        level.Denomination.First(),
                        level,
                        new AssignableProgressiveId(AssignableProgressiveType.Linked, linkedLevel.LevelName),
                        level.ResetValue)).ToList(),
                        ProtocolNames.Bingo);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _eventBus.UnsubscribeAll(this);
            _pendingAwardsLock.Dispose();
            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _multiProtocolEventBusRegistry.SubscribeProgressiveEvent<LinkedProgressiveHitEvent>(ProtocolNames.Bingo, this);
            _eventBus.Subscribe<PendingLinkedProgressivesHitEvent>(this, (e, _) => Handle(e, CancellationToken.None));
            _eventBus.Subscribe<PaytablesInstalledEvent>(this, Handle);
            _eventBus.Subscribe<ProtocolInitialized>(this, Handle);
            _eventBus.Subscribe<ProgressiveContributionEvent>(this, (e, _) => Handle(e, CancellationToken.None));
        }

        private bool IsActiveGame(int gameTitleId, long denom)
        {
            var activeGame = _gameProvider.GetActiveGame();

            // Check if the active main game a match
            if (gameTitleId == Convert.ToInt32(activeGame.game.CdsTitleId) && denom == activeGame.denomination.Value)
            {
                return true;
            }

            // Check if the active sub game is a match
            var activeSubGames = activeGame.game.ActiveSubGames;
            if (activeSubGames is null)
            {
                return false;
            }

            foreach (var subGame in activeSubGames)
            {
                if (gameTitleId == Convert.ToInt32(subGame.CdsTitleId) &&
                    subGame.ActiveDenoms is not null &&
                    subGame.ActiveDenoms.Any(activeDenom => denom == activeDenom))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task Handle(ProgressiveContributionEvent evt, CancellationToken token)
        {
            var machineSerial = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);

            if (string.IsNullOrEmpty(machineSerial))
            {
                Logger.Error($"Unable to get {ApplicationConstants.SerialNumber} from properties manager");
                return;
            }

            var levelId = 0;
            foreach (var wager in evt.Wagers)
            {
                var gamesUsingProgressive = _progressiveContributionService.GetGamesUsingProgressive(levelId);
                foreach (var (gameTitleId, denomination) in gamesUsingProgressive.Result)
                {
                    if (IsActiveGame(gameTitleId, denomination))
                    {
                        var message = new ProgressiveContributionRequestMessage(
                            wager,
                            machineSerial,
                            gameTitleId,
                            false, // Not used with server 11
                            false, // Not used with server 11
                            (int)denomination);
                        await _progressiveContributionService.Contribute(message, token);

                        Logger.Debug($"Game [GameTitleId={gameTitleId}, Denom={denomination}] is contributing to progressive {levelId} a wager amount of {wager}");
                    }
                }

                ++levelId;
            }
        }

        private void Handle(PaytablesInstalledEvent evt)
        {
            // On an initial boot must configure after paytables are installed. This event is only sent on initial boot.
            Configure();
        }

        private void Handle(ProtocolInitialized evt)
        {
            // On normal boot configure when the protocol is initialized
            Configure();
        }

        private async Task Handle(PendingLinkedProgressivesHitEvent evt, CancellationToken token)
        {
            Logger.Info($"Received PendingLinkedProgressivesHitEvent with {evt.LinkedProgressiveLevels.ToList().Count} progressive levels");

            if (evt.LinkedProgressiveLevels.ToList().Count > 0)
            {
                Logger.Debug("Progressive levels hit:");
                foreach (var level in evt.LinkedProgressiveLevels.ToList())
                {
                    Logger.Debug($"Progressive Level: name={level.LevelName} levelId={level.LevelId} amount={level.Amount}");
                }
            }

            var machineSerial = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);

            if (string.IsNullOrEmpty(machineSerial))
            {
                Logger.Error($"Unable to get {ApplicationConstants.SerialNumber} from properties manager");
                return;
            }

            var tasks = new List<Task>();
            await _pendingAwardsLock.WaitAsync(token);
            try
            {
                foreach (var level in evt.LinkedProgressiveLevels)
                {
                    // Calls to progressive server to claim each progressive level.
                    Logger.Debug(
                        $"Calling ProgressiveClaimService.ClaimProgressive, MachineSerial={machineSerial}, ProgLevelId={level.LevelId}, Amount={level.Amount}");
                    var response = _progressiveClaimService.ClaimProgressive(
                        new ProgressiveClaimRequestMessage(machineSerial, level.LevelId, level.Amount), token);
                    Logger.Debug(
                        $"ProgressiveClaimResponse received, ResponseCode={response.Result.ResponseCode} ProgressiveLevelId={response.Result.ProgressiveLevelId}, ProgressiveWinAmount={response.Result.ProgressiveWinAmount}, ProgressiveAwardId={response.Result.ProgressiveAwardId}");
                    tasks.Add(response);
                    await _bingoGameOutcomeHandler.ProcessProgressiveClaimWin(
                        response.Result.ProgressiveWinAmount.CentsToMillicents());

                    // TODO copied code does not allow for multiple pending awards with the same pool name. For Bingo we allow hitting the same progressive multiple times. How to handle?
                    var poolName = GetPoolName(level.LevelName);
                    if (!string.IsNullOrEmpty(poolName) &&
                        !_pendingAwards.Any(a => a.poolName.Equals(poolName, StringComparison.OrdinalIgnoreCase)))
                    {
                        _pendingAwards.Add(
                            (poolName, response.Result.ProgressiveLevelId, response.Result.ProgressiveWinAmount,
                                response.Result.ProgressiveAwardId));
                    }
                }
            }
            finally
            {
                _pendingAwardsLock.Release();
            }

            await Task.WhenAll(tasks);
        }

        private async Task Handle(LinkedProgressiveHitEvent evt, CancellationToken token)
        {
            Logger.Debug($"LinkedProgressiveHitEvent Handler with level Id {evt.Level.LevelId}");
            var linkedLevel = evt.LinkedProgressiveLevels.FirstOrDefault();

            if (linkedLevel is null)
            {
                Logger.Error($"Cannot find linkedLevel for level Id {evt.Level.LevelId} with wager credits {evt.Level.WagerCredits}");
                return;
            }

            // Must get the win amount out of the pending awards. The value in the event will not be correct.
            var winAmount = 0L;
            var matchPoolName = GetPoolName(linkedLevel.LevelName);

            await _pendingAwardsLock.WaitAsync(token);
            try
            {
                (string, long, long, int) pendingAwardToRemove = new();
                var matched = false;
                foreach (var pendingAward in _pendingAwards)
                {
                    if (pendingAward.poolName == matchPoolName)
                    {
                        winAmount = pendingAward.amountInPennies;
                        pendingAwardToRemove = pendingAward;
                        matched = true;
                        break;
                    }
                }

                if (matched)
                {
                    _pendingAwards.Remove(pendingAwardToRemove);
                }
            }
            finally
            {
                _pendingAwardsLock.Release();
            }

            Logger.Debug(
                $"AwardJackpot progressiveLevel = {evt.Level.LevelName} " +
                $"linkedLevel = {linkedLevel.LevelName} " +
                $"amountInPennies = {winAmount} " +
                $"CurrentValue = {evt.Level.CurrentValue}");

            _protocolLinkedProgressiveAdapter.ClaimLinkedProgressiveLevel(linkedLevel.LevelName, ProtocolNames.Bingo);
            var poolName = GetPoolName(linkedLevel.LevelName);
            await AwardJackpot(poolName, winAmount, token);
            _protocolLinkedProgressiveAdapter.AwardLinkedProgressiveLevel(linkedLevel.LevelName, winAmount, ProtocolNames.Bingo);
        }

        private LinkedProgressiveLevel UpdateLinkedProgressiveLevels(
            int progId,
            int levelId,
            long valueInCents,
            bool initialize = false)
        {
            var linkedLevel = LinkedProgressiveLevel(progId, levelId, valueInCents);

            if (initialize && _gameHistory.IsRecoveryNeeded)
            {
                return linkedLevel;
            }

            if (!initialize || !_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Any(l => l.LevelName.Equals(linkedLevel.LevelName, StringComparison.OrdinalIgnoreCase)))
            {
                _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                    new[] { linkedLevel },
                    ProtocolNames.Bingo);
            }

            Logger.Info(
                $"Updated linked progressive level: ProtocolName={linkedLevel.ProtocolName} ProgressiveGroupId={linkedLevel.ProgressiveGroupId} LevelName={linkedLevel.LevelName} LevelId={linkedLevel.LevelId} Amount={linkedLevel.Amount} ClaimStatus={linkedLevel.ClaimStatus} CurrentErrorStatus={linkedLevel.CurrentErrorStatus} Expiration={linkedLevel.Expiration}");

            return linkedLevel;
        }

        private string CreatePoolName(string packName, string levelName, long resetValueMillicents)
        {
            // These are the naming conventions for NYL but it should be ok to re-use for bingo
            return $"{packName}_{resetValueMillicents.MillicentsToDollars()}_{levelName}";
        }

        private string GetPoolName(string levelName)
        {
            var key = _progressives.FirstOrDefault(p => p.Value.Any(i => LevelName(i).Equals(levelName, StringComparison.OrdinalIgnoreCase))).Key;
            return key ?? string.Empty;
        }

        private static LinkedProgressiveLevel LinkedProgressiveLevel(int progId, int levelId, long valueInCents)
        {
            var linkedLevel = new LinkedProgressiveLevel
            {
                ProtocolName = ProtocolNames.Bingo,
                ProgressiveGroupId = progId,
                LevelId = levelId,
                Amount = valueInCents,
                Expiration = DateTime.UtcNow + TimeSpan.FromDays(365),
                CurrentErrorStatus = ProgressiveErrors.None
            };

            return linkedLevel;
        }

        private static string LevelName(ProgressiveInfo info)
        {
            return $"{ProtocolNames.Bingo}, LevelId: {info.LevelId}, ProgressiveGroupId: {info.ProgId}";
        }
    }
}