namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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
        private readonly object _pendingAwardsLock = new();
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
            IBingoGameOutcomeHandler bingoGameOutcomeHandler
            )
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
            Handle(@event as LinkedProgressiveHitEvent);
        }

        /// <inheritdoc />
        public void AwardJackpot(string poolName, long amountInPennies)
        {
            if (_gameHistory?.CurrentLog.PlayState == PlayState.Idle)
            {
                return;
            }

            if (string.IsNullOrEmpty(poolName) || amountInPennies <= 0)
            {
                return;
            }

            IViewableProgressiveLevel level = null;
            foreach (var l in _protocolLinkedProgressiveAdapter.ViewConfiguredProgressiveLevels())
            {
                var levelPoolName = CreatePoolName(l.ProgressivePackName, l.LevelName, l.ResetValue);
                if (levelPoolName == poolName)
                {
                    level = l;
                    break;
                }
            }

            if (level is null)
            {
                throw new InvalidOperationException($"No progressive levels found matching '{poolName}'");
            }

            var levelId = level.LevelId;
            var progressiveId = level.ProgressiveId;
            lock (_pendingAwardsLock)
            {
                // add to pending if another level is hit
                if (_pendingAwards.All(x => x.progressiveLevelId != levelId))
                {
                    Logger.Info($"Adding pending linked level for {poolName} amount={amountInPennies} LevelId={levelId} awardId={progressiveId}");
                    _pendingAwards!.Add((poolName, levelId, amountInPennies, progressiveId));

                    UpdatePendingAwards();

                    return;
                }

                var machineSerial = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);

                var request = new ProgressiveAwardRequestMessage(
                    machineSerial,
                    progressiveId,
                    levelId,
                    amountInPennies,
                    true);
                _progressiveAwardService.AwardProgressive(request);
            }
        }

        /// <inheritdoc />
        public void Configure()
        {
            Logger.Debug("ProgressiveController Configure");

            _progressives.Clear();
            _activeProgressiveInfos.Clear();

            // handle if there are pending awards
            lock (_pendingAwardsLock)
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

            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _multiProtocolEventBusRegistry.SubscribeProgressiveEvent<LinkedProgressiveHitEvent>(ProtocolNames.Bingo, this);
            _eventBus.Subscribe<PendingLinkedProgressivesHitEvent>(this, Handle);
            _eventBus.Subscribe<PaytablesInstalledEvent>(this, Handle);
            _eventBus.Subscribe<ProtocolInitialized>(this, Handle);
            _eventBus.Subscribe<ProgressiveContributionEvent>(this, Handle);
        }

        private void Handle(ProgressiveContributionEvent evt)
        {
            var machineSerial = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);

            if (string.IsNullOrEmpty(machineSerial))
            {
                Logger.Error($"Unable to get {ApplicationConstants.SerialNumber} from properties manager");
                return;
            }

            int levelId = 0;
            foreach (var wager in evt.Wagers)
            {
                var gamesUsingProgressive = _progressiveContributionService.GetGamesUsingProgressive(levelId);
                foreach (var game in gamesUsingProgressive.Result)
                {
                    var gameTitleId = game.Item1;
                    var denomination = game.Item2;
                    var message = new ProgressiveContributionRequestMessage(
                        wager,
                        machineSerial,
                        gameTitleId,
                        false, // Not used with server 11
                        false, // Not used with server 11
                        (int)denomination);
                    _progressiveContributionService.Contribute(message);

                    Logger.Debug($"Game [GameTitleId={gameTitleId}, Denom={denomination}] is contributing to progressive {levelId} a wager amount of {wager}");

                    ++levelId;
                }
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

        private void Handle(PendingLinkedProgressivesHitEvent evt)
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

            lock (_pendingAwardsLock)
            {
                foreach (var level in evt.LinkedProgressiveLevels)
                {
                    // Calls to progressive server to claim each progressive level.
                    Logger.Debug($"Calling ProgressiveClaimService.ClaimProgressive, MachineSerial={machineSerial}, ProgLevelId={level.LevelId}, Amount={level.Amount}");
                    var response = _progressiveClaimService.ClaimProgressive(new ProgressiveClaimRequestMessage(machineSerial, level.LevelId, level.Amount));
                    Logger.Debug($"ProgressiveClaimResponse received, ResponseCode={response.Result.ResponseCode} ProgressiveLevelId={response.Result.ProgressiveLevelId}, ProgressiveWinAmount={response.Result.ProgressiveWinAmount}, ProgressiveAwardId={response.Result.ProgressiveAwardId}");

                    _bingoGameOutcomeHandler.ProcessProgressiveClaimWin(response.Result.ProgressiveWinAmount.CentsToMillicents());

                    // TODO copied code does not allow for multiple pending awards with the same pool name. For Bingo we allow hitting the same progressive multiple times. How to handle?
                    var poolName = GetPoolName(level.LevelName);
                    if (!string.IsNullOrEmpty(poolName) &&
                        !_pendingAwards.Any(a => a.poolName.Equals(poolName, StringComparison.OrdinalIgnoreCase)))
                    {
                        _pendingAwards.Add((poolName, response.Result.ProgressiveLevelId, response.Result.ProgressiveWinAmount, response.Result.ProgressiveAwardId));
                    }

                    // TODO will need to persist the _pending award value in persistent storage
                }
            }
        }

        private void Handle(LinkedProgressiveHitEvent evt)
        {
            Logger.Debug($"LinkedProgressiveHitEvent Handler with level Id {evt.Level.LevelId}");
            var linkedLevel = evt.LinkedProgressiveLevels.FirstOrDefault();

            if (linkedLevel is null)
            {
                Logger.Error($"Cannot find linkedLevel for level Id {evt.Level.LevelId} with wager credits {evt.Level.WagerCredits}");
                return;
            }

            Logger.Debug(
                $"AwardJackpot progressiveLevel = {evt.Level.LevelName} " +
                $"linkedLevel = {linkedLevel.LevelName} " +
                $"amountInPennies = {linkedLevel.ClaimStatus.WinAmount} " +
                $"CurrentValue = {evt.Level.CurrentValue}");

            _protocolLinkedProgressiveAdapter.ClaimLinkedProgressiveLevel(linkedLevel.LevelName, ProtocolNames.Bingo);
            var poolName = GetPoolName(linkedLevel.LevelName);
            AwardJackpot(poolName, linkedLevel.ClaimStatus.WinAmount);
            _protocolLinkedProgressiveAdapter.AwardLinkedProgressiveLevel(linkedLevel.LevelName, linkedLevel.ClaimStatus.WinAmount, ProtocolNames.Bingo);
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

        private void UpdatePendingAwards()
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var pendingJackpots = unitOfWork.Repository<PendingJackpotAwards>().Queryable().SingleOrDefault() ??
                                  new PendingJackpotAwards();

            pendingJackpots.Awards = JsonConvert.SerializeObject(_pendingAwards);

            unitOfWork.Repository<PendingJackpotAwards>().AddOrUpdate(pendingJackpots);

            unitOfWork.SaveChanges();
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
            return $"{ProtocolNames.Bingo}, Level Id: {info.LevelId}, Progressive Group Id: {info.ProgId}";
        }
    }
}