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
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

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
        private readonly IPersistentStorageManager _storage;
        private readonly IGameHistory _gameHistory;
        //private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IProgressiveClaimService _progressiveClaimService;
        private readonly IPropertiesManager _propertiesManager;

        private readonly ConcurrentDictionary<string, IList<ProgressiveInfo>> _progressives = new();
        private readonly IList<ProgressiveInfo> _activeProgressiveInfos = new List<ProgressiveInfo>();
        //private readonly HashSet<string> _progressiveMessageAttributes = new();
        private readonly object _pendingAwardsLock = new();
        //private readonly IList<(string poolName, long amountInPennies)> _pendingAwards = new List<(string, long)>();
        private readonly IList<(string poolName, long progressiveLevelId, long amountInPennies, int awardId)> _pendingAwards = new List<(string, long, long, int)>();
        private string _updateProgressiveMeter;
        private bool _disposed;
        private IEnumerable<IViewableLinkedProgressiveLevel> _currentLinkedProgressiveLevelsHit;
        private bool _progressiveRecovery;

        //private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveController" /> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus" />.</param>
        /// <param name="gameProvider"><see cref="IGameProvider" />.</param>
        /// <param name="protocolLinkedProgressiveAdapter">.</param>
        /// <param name="storage"><see cref="IPersistentStorageManager" />.</param>
        /// <param name="gameHistory"><see cref="IGameHistory" />.</param>
        /// <param name="multiProtocolEventBusRegistry"><see cref="IProtocolProgressiveEventsRegistry" />.</param>
        /// <param name="progressiveClaimService"><see cref="IProgressiveClaimService" />.</param>
        /// <param name="propertiesManager"><see cref="IPropertiesManager" />.</param>
        public ProgressiveController(
            IEventBus eventBus,
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPersistentStorageManager storage,
            IGameHistory gameHistory,
            //IUnitOfWorkFactory unitOfWorkFactory,
            IProtocolProgressiveEventsRegistry multiProtocolEventBusRegistry,
            IProgressiveClaimService progressiveClaimService,
            IPropertiesManager propertiesManager
            )
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                                throw new ArgumentNullException(
                                                    nameof(protocolLinkedProgressiveAdapter));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            //_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _multiProtocolEventBusRegistry = multiProtocolEventBusRegistry ??
                                             throw new ArgumentNullException(nameof(multiProtocolEventBusRegistry));
            _progressiveClaimService = progressiveClaimService ?? throw new ArgumentNullException(nameof(progressiveClaimService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

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
            //if (_gameHistory?.CurrentLog.PlayState == PlayState.Idle)
            //{
            //    return;
            //}

            //if (!string.IsNullOrEmpty(poolName) && amountInPennies > 0)
            //{
            //    lock (_pendingAwardsLock)
            //    {
            //        var valueAttributeName = $"ins{poolName}_Value";
            //        var level = _currentLinkedProgressiveLevelsHit?.FirstOrDefault();

            //         add to pending if current level is null or if another level is hit
            //        if (level == null || !_pendingAwards.Any(
            //                a => a.poolName.Equals(valueAttributeName)))
            //        {
            //            Logger.Info($"No current pending linked level for {poolName}");

            //            var replaceAward = _pendingAwards.FirstOrDefault(
            //                a => a.poolName.Equals(valueAttributeName) && a.amountInPennies.Equals(0));

            //            if (replaceAward != default((string, long)))
            //            {
            //                _pendingAwards.Remove(replaceAward);
            //            }

            //            Logger.Info($"Adding pending linked level for {valueAttributeName} {amountInPennies}");

            //            _pendingAwards.Add((valueAttributeName, amountInPennies));

            //            UpdatePendingAwards();

            //            return;
            //        }

            //        AwardJackpotLevel(amountInPennies, level.LevelName, valueAttributeName);
            //    }
            //}
        }

        /// <inheritdoc />
        public IList<ProgressiveInfo> GetActiveProgressives()
        {
            return _activeProgressiveInfos;
        }

        /// <inheritdoc />
        public void Configure()
        {
            Logger.Debug("ProgressiveController Configure");

            _progressives.Clear();
            _activeProgressiveInfos.Clear();
            //if (_pendingAwards == null)
            //{
            //    using (var unitOfWork = _unitOfWorkFactory.Create())
            //    {
            //        var pendingJackpots = unitOfWork.Repository<PendingJackpotAwards>().Queryable().SingleOrDefault();
            //        _pendingAwards = pendingJackpots?.Awards == null
            //            ? new List<(string, long)>()
            //            : JsonConvert.DeserializeObject<IList<(string, long)>>(pendingJackpots.Awards);
            //    }

            //    CheckProgressiveRecovery();
            //}

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
                if (game == null)
                {
                    continue;
                }

                var resetValue = pool.First().ResetValue;

                var poolName = $"{pool.Key.PackName}_{resetValue.MillicentsToDollars()}_{pool.Key.LevelName}";

                var valueAttributeName = $"ins{poolName}_Value";
                var messageAttributeName = $"ins{poolName}_Message";
                //_progressiveMessageAttributes.Add(messageAttributeName);

                var progressive = new ProgressiveInfo(
                    pool.Key.PackName,
                    pool.Key.ProgId,
                    pool.Key.LevelId,
                    pool.Key.LevelName,
                    poolName,
                    valueAttributeName,
                    messageAttributeName);

                if (!_progressives.ContainsKey(valueAttributeName))
                {
                    _progressives.TryAdd(valueAttributeName, new List<ProgressiveInfo>());
                }

                if (game.EgmEnabled &&
                    !_activeProgressiveInfos.Any(p => p.ValueAttributeName.Equals(progressive.ValueAttributeName, StringComparison.OrdinalIgnoreCase)))
                {
                    _activeProgressiveInfos.Add(progressive);
                }

                _progressives[valueAttributeName].Add(progressive);

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

        private void UpdatePendingAwards()
        {
            // TODO should this store the values in nvram? Is that what this MGAM code is doing?

            //using (var unitOfWork = _unitOfWorkFactory.Create())
            //{
            //    var pendingJackpots = unitOfWork.Repository<PendingJackpotAwards>().Queryable().SingleOrDefault() ??
            //                          new PendingJackpotAwards();

            //    pendingJackpots.Awards = JsonConvert.SerializeObject(_pendingAwards);

            //    unitOfWork.Repository<PendingJackpotAwards>().AddOrUpdate(pendingJackpots);

            //    unitOfWork.SaveChanges();
            //}
        }

        private void AwardJackpotLevel(
            long amountInPennies,
            string levelName,
            string attributePropertyName)
        {
            if (_progressiveRecovery)
            {
                amountInPennies = JackpotAmountInPennies();
            }

            if (_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(levelName, out var level) &&
                level.ClaimStatus.Status == LinkedClaimState.Hit)
            {
                using (var scope = _storage.ScopedTransaction())
                {
                    Logger.Info($"AwardJackpot {levelName} amountInPennies {amountInPennies}");
                    _protocolLinkedProgressiveAdapter.ClaimLinkedProgressiveLevel(
                        levelName,
                        ProtocolNames.Bingo);
                    _protocolLinkedProgressiveAdapter.AwardLinkedProgressiveLevel(
                        levelName,
                        amountInPennies,
                        ProtocolNames.Bingo);

                    _updateProgressiveMeter = attributePropertyName;

                    scope.Complete();
                }
            }

            lock (_pendingAwardsLock)
            {
                var award = _pendingAwards.FirstOrDefault(
                    a => a.poolName.Equals(attributePropertyName, StringComparison.OrdinalIgnoreCase) &&
                         (a.amountInPennies == amountInPennies || a.amountInPennies == 0));
                if (award != default((string, long, long, int)))
                {
                    _pendingAwards.Remove(award);

                    UpdatePendingAwards();

                    if (!_pendingAwards.Any())
                    {
                        UpdateProgressiveValues();
                    }
                }

                _currentLinkedProgressiveLevelsHit = null;
            }
        }

        private long JackpotAmountInPennies()
        {
            //var result = _currentLinkedProgressiveLevelsHit?.FirstOrDefault()?.Amount ?? 0;

            //if (result == 0)
            //{
            //    var claimedJackpotTotal =
            //        _gameHistory?.CurrentLog.Jackpots.Sum(j => j.WinAmount.MillicentsToCents()) ?? 0;

            //    var totalJackpots = _gameHistory?.CurrentLog.Outcomes.LastOrDefault();

            //    if (totalJackpots != null && _pendingAwards.Count >= 1)
            //    {
            //        return totalJackpots.Value.MillicentsToCents() - claimedJackpotTotal;
            //    }
            //}

            //return result;
            return 0L;
        }

        private bool AllJackpotsClaimed()
        {
            var claimedJackpotTotal =
                _gameHistory?.CurrentLog.Jackpots.Sum(j => j.WinAmount.MillicentsToCents()) ?? 0;

            if (claimedJackpotTotal == 0)
            {
                return false;
            }

            var totalJackpots = _gameHistory?.CurrentLog.Outcomes.LastOrDefault();

            return totalJackpots?.Value.MillicentsToCents() - claimedJackpotTotal == 0;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<HostDisconnectedEvent>(this, Handle);
            _eventBus.Subscribe<ProgressiveHostOfflineEvent>(this, Handle);
            _multiProtocolEventBusRegistry.SubscribeProgressiveEvent<LinkedProgressiveHitEvent>(
                ProtocolNames.Bingo,
                this);
            _eventBus.Subscribe<GamePlayStateChangedEvent>(this, Handle);
            _eventBus.Subscribe<GameProcessExitedEvent>(this, _ => CheckProgressiveRecovery());
            _eventBus.Subscribe<WaitingForPlayerInputStartedEvent>(this, Handle);
            _eventBus.Subscribe<PendingLinkedProgressivesHitEvent>(this, Handle);
            _eventBus.Subscribe<GameInitializationCompletedEvent>(this, _ => UpdateProgressiveValues());
            _eventBus.Subscribe<LinkedProgressiveResetEvent>(this, _ => UpdateProgressiveMeter());
            _eventBus.Subscribe<ProtocolInitialized>(this, Handle);
        }

        private void Handle(ProtocolInitialized evt)
        {
            Configure();
        }

        private void Handle(HostDisconnectedEvent evt)
        {
            //CheckProgressiveRecovery();
            //lock (_pendingAwardsLock)
            //{
            //    if (_progressiveRecovery && _currentLinkedProgressiveLevelsHit != null)
            //    {
            //        ProcessProgressiveLevels(_currentLinkedProgressiveLevelsHit);
            //    }
            //}
        }

        private void Handle(ProgressiveHostOfflineEvent evt)
        {
            // TODO
        }

        private void Handle(PendingLinkedProgressivesHitEvent evt)
        {
            Logger.Info("Received PendingLinkedProgressivesHitEvent");
            Logger.Debug("PendingLinkedProgressivesHitEvent progressiveLevels:");
            foreach (var level in evt.LinkedProgressiveLevels.ToList())
            {
                Logger.Debug($"levelId = name={level.LevelName} levelId={level.LevelId} amount={level.Amount}");
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

                    // TODO copied code does not allow for multiple pending awards with the same pool name. For Bingo we allow hitting the same progressive multiple times. How to handle?
                    var poolName = GetPoolName(level.LevelName);
                    if (!string.IsNullOrEmpty(poolName) &&
                        (!_pendingAwards.Any(a => a.poolName.Equals(poolName, StringComparison.OrdinalIgnoreCase))))
                    {
                        _pendingAwards.Add((poolName, response.Result.ProgressiveLevelId, response.Result.ProgressiveWinAmount, response.Result.ProgressiveAwardId));
                    }

                    UpdatePendingAwards();
                }
            }
        }

        private void Handle(WaitingForPlayerInputStartedEvent evt)
        {
            UpdateProgressiveMeter();

            _updateProgressiveMeter = null;
        }

        private void Handle(GamePlayStateChangedEvent evt)
        {
            if (evt.CurrentState == PlayState.PayGameResults)
            {
                lock (_pendingAwardsLock)
                {
                    if (_pendingAwards != null && _pendingAwards.Count != 0)
                    {
                        _pendingAwards.Clear();
                        UpdatePendingAwards();
                    }

                    if (_progressiveRecovery)
                    {
                        UpdateProgressiveValues();
                    }

                    _progressiveRecovery = false;
                }
            }
        }

        private void Handle(LinkedProgressiveHitEvent evt)
        {
            if (_pendingAwards.Count > 0)
            {
                foreach (var (poolName, progressiveLevelId, amountInPennies, awardId) in _pendingAwards)
                {
                    if (!_progressives.ContainsKey(poolName) || amountInPennies == 0)
                    {
                        continue;
                    }

                    foreach (var level in _progressives[poolName])
                    {
                        var levelName = LevelName(level);
                        if (!levelName.Equals(evt.LinkedProgressiveLevels.First().LevelName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        AwardJackpotLevel(amountInPennies, levelName, poolName);
                        return;
                    }
                }
            }

            lock (_pendingAwardsLock)
            {
                if (_progressiveRecovery)
                {
                    ProcessProgressiveLevels(evt.LinkedProgressiveLevels);

                    return;
                }

                if (_pendingAwards.Count > 0)
                {
                    foreach (var (poolName, progressiveLevelId, amountInPennies, awardId) in _pendingAwards)
                    {
                        if (!_progressives.ContainsKey(poolName) || amountInPennies == 0)
                        {
                            continue;
                        }

                        foreach (var level in _progressives[poolName])
                        {
                            var levelName = LevelName(level);
                            if (!levelName.Equals(evt.LinkedProgressiveLevels.First().LevelName, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            AwardJackpotLevel(amountInPennies, levelName, poolName);
                            return;
                        }
                    }
                }

                _currentLinkedProgressiveLevelsHit = evt.LinkedProgressiveLevels;
            }
        }

        private void UpdateProgressiveMeter()
        {
            if (!string.IsNullOrEmpty(_updateProgressiveMeter)&&
                _progressives.TryGetValue(_updateProgressiveMeter, out var progressiveInfos))
            {
                // TODO Bingo doesn't have any attributes. How to handle this.
                //var value = _attributes.Get(_updateProgressiveMeter, 0);
                var value = 0;

                foreach (var progressiveInfo in progressiveInfos)
                {
                    UpdateLinkedProgressiveLevels(
                        progressiveInfo.ProgId,
                        progressiveInfo.LevelId,
                        value
                    );
                }
            }
        }

        private void CheckProgressiveRecovery()
        {
            if (_gameHistory?.CurrentLog?.PlayState != PlayState.Idle &&
                _gameHistory?.CurrentLog?.Outcomes.Count() > 1 &&
                (JackpotAmountInPennies() != 0 || AllJackpotsClaimed()))
            {
                _progressiveRecovery = true;
            }
        }

        private void UpdateProgressiveValues()
        {
            //foreach (var progressiveInfos in _progressives)
            //{
            //    // TODO Bingo doesn't have any attributes. How to handle this.
            //    //var value = _attributes.Get(progressiveInfos.Key, 0);
            //    var value = 0;

            //    if (value > 0)
            //    {
            //        foreach (var progressive in progressiveInfos.Value)
            //        {
            //            UpdateLinkedProgressiveLevels(
            //                progressive.ProgId,
            //                progressive.LevelId,
            //                value
            //            );
            //        }
            //    }
            //}
        }

        private void ProcessProgressiveLevels(IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            var awards = levels.ToList();
            foreach (var progressiveInfos in _progressives.Values)
            {
                foreach (var progressiveInfo in progressiveInfos)
                {
                    var award = awards.FirstOrDefault(
                        p => progressiveInfo.LevelId == p.LevelId &&
                             progressiveInfo.ProgId == p.ProgressiveGroupId);

                    if (award == null)
                    {
                        continue;
                    }

                    AwardJackpotLevel(0, award.LevelName, progressiveInfo.ValueAttributeName);
                    awards.Remove(award);
                    if (awards.Count == 0)
                    {
                        return;
                    }
                }
            }
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
            return $"{ProtocolNames.Bingo}, Level Id: {info.LevelId}, Progressive Group Id: {info.ProgId}";
        }
    }
}