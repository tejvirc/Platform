namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Common;
    using Common.Data.Models;
    using Common.Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
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
        private readonly IPersistentStorageManager _storage;
        private readonly IGameHistory _gameHistory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        private readonly ConcurrentDictionary<string, IList<ProgressiveInfo>> _progressives = new();
        private readonly IList<ProgressiveInfo> _activeProgressiveInfos = new List<ProgressiveInfo>();
        private readonly HashSet<string> _progressiveMessageAttributes = new();
        private readonly object _pendingAwardsLock = new();
        //private IList<(string poolName, long amountInPennies)> _pendingAwards;
        private string _updateProgressiveMeter;
        private bool _disposed;
        private IEnumerable<IViewableLinkedProgressiveLevel> _currentLinkedProgressiveLevelsHit;
        private bool _progressiveRecovery;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveController" /> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus" />.</param>
        /// <param name="gameProvider"><see cref="IGameProvider" />.</param>
        /// <param name="protocolLinkedProgressiveAdapter">.</param>
        /// <param name="storage"><see cref="IPersistentStorageManager" />.</param>
        /// <param name="gameHistory"><see cref="IGameHistory" />.</param>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory" />.</param>
        /// <param name="multiProtocolEventBusRegistry"><see cref="IProtocolProgressiveEventsRegistry" />.</param>
        public ProgressiveController(
            IEventBus eventBus,
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPersistentStorageManager storage,
            IGameHistory gameHistory,
            IUnitOfWorkFactory unitOfWorkFactory,
            IProtocolProgressiveEventsRegistry multiProtocolEventBusRegistry)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                                throw new ArgumentNullException(
                                                    nameof(protocolLinkedProgressiveAdapter));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _multiProtocolEventBusRegistry = multiProtocolEventBusRegistry ??
                                             throw new ArgumentNullException(nameof(multiProtocolEventBusRegistry));

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

            //            // add to pending if current level is null or if another level is hit
            //            if (level == null || !_pendingAwards.Any(
            //                a => a.poolName.Equals(valueAttributeName)))
            //            {
            //                //_logger.LogInfo($"No current pending linked level for {poolName}");

            //                    var replaceAward = _pendingAwards.FirstOrDefault(
            //                    a => a.poolName.Equals(valueAttributeName) && a.amountInPennies.Equals(0));

            //                    if (replaceAward != default((string, long)))
            //                    {
            //                        _pendingAwards.Remove(replaceAward);
            //                    }

            //                //_logger.LogInfo($"Adding pending linked level for {valueAttributeName} {amountInPennies}");

            //                _pendingAwards.Add((valueAttributeName, amountInPennies));

            //                    UpdatePendingAwards();

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
            Logger.Debug("SGL ProgressiveController Configure");

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

            Logger.Debug($"SGL ViewProgressiveLevels count = {_protocolLinkedProgressiveAdapter.ViewProgressiveLevels().Count()}");

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

            Logger.Debug($"SGL progressive pools count = {pools.Length}");

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
                _progressiveMessageAttributes.Add(messageAttributeName);

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
                    !_activeProgressiveInfos.Any(p => p.ValueAttributeName.Equals(progressive.ValueAttributeName)))
                {
                    _activeProgressiveInfos.Add(progressive);
                }

                _progressives[valueAttributeName].Add(progressive);

                Logger.Debug("SGL calling UpdateLinkedProgressiveLevels");

                var linkedLevel = UpdateLinkedProgressiveLevels(
                    pool.Key.ProgId,
                    pool.Key.LevelId,
                    resetValue.MillicentsToCents(),
                    true);

                Logger.Debug("SGL calling _protocolLinkedProgressiveAdapter.AssignLevelsToGame");

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
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                //var pendingJackpots = unitOfWork.Repository<PendingJackpotAwards>().Queryable().SingleOrDefault() ??
                //                      new PendingJackpotAwards();

                //pendingJackpots.Awards = JsonConvert.SerializeObject(_pendingAwards);

                //unitOfWork.Repository<PendingJackpotAwards>().AddOrUpdate(pendingJackpots);

                unitOfWork.SaveChanges();
            }
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

            if (_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(levelName, out var level)&&
                level.ClaimStatus.Status == LinkedClaimState.Hit)
            {
                using (var scope = _storage.ScopedTransaction())
                {
                    //_logger.LogInfo($"AwardJackpot {levelName} amountInPennies {amountInPennies}");
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

            //lock (_pendingAwardsLock)
            //{
            //    var award = _pendingAwards.FirstOrDefault(
            //        a => a.poolName.Equals(attributePropertyName) &&
            //             (a.amountInPennies == amountInPennies || a.amountInPennies == 0));
            //    if(award != default((string, long)))
            //    {
            //        _pendingAwards.Remove(award);

            //        UpdatePendingAwards();

            //        if (!_pendingAwards.Any())
            //        {
            //            UpdateProgressiveValues();
            //        }
            //    }

            //    _currentLinkedProgressiveLevelsHit = null;
            //}
        }

        private long JackpotAmountInPennies()
        {
            var result = _currentLinkedProgressiveLevelsHit?.FirstOrDefault()?.Amount ?? 0;

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

            return result;
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
            _multiProtocolEventBusRegistry.SubscribeProgressiveEvent<LinkedProgressiveHitEvent>(
                ProtocolNames.Bingo,
                this);
            _eventBus.Subscribe<GamePlayStateChangedEvent>(this, Handle);
            _eventBus.Subscribe<GameProcessExitedEvent>(this, _ => CheckProgressiveRecovery());
            _eventBus.Subscribe<WaitingForPlayerInputStartedEvent>(this, Handle);
            _eventBus.Subscribe<PendingLinkedProgressivesHitEvent>(this, Handle);
            _eventBus.Subscribe<GameInitializationCompletedEvent>(this, _ => UpdateProgressiveValues());
            _eventBus.Subscribe<LinkedProgressiveResetEvent>(this, _ => UpdateProgressiveMeter());
            _eventBus.Subscribe<ProtocolsInitializedEvent>(this, Handle);
        }

        private void Handle(ProtocolsInitializedEvent evt)
        {
            Logger.Debug("SGL Handle ProtocolsInitializedEvent, configuring...");
            Configure();
        }

        private void Handle(HostDisconnectedEvent evt)
        {
            CheckProgressiveRecovery();
            lock (_pendingAwardsLock)
            {
                if (_progressiveRecovery && _currentLinkedProgressiveLevelsHit != null)
                {
                    ProcessProgressiveLevels(_currentLinkedProgressiveLevelsHit);
                }
            }
        }

        private void Handle(PendingLinkedProgressivesHitEvent evt)
        {
            //_logger.LogInfo($"Received PendingLinkedProgressivesHitEvent {evt}");

            //lock (_pendingAwardsLock)
            //{
                //foreach (var level in evt.LinkedProgressiveLevels)
                //{
                //    var poolName = GetPoolName(level.LevelName);
                //    if (!string.IsNullOrEmpty(poolName)&&           // we cannot guarantee the amount, so set to 0 and the host should send the correct one or recovery will trigger
                //        (!_pendingAwards.Any(
                //            a => a.poolName.Equals(poolName))))
                //    {
                //        _pendingAwards.Add((poolName, 0));
                //    }

                //    UpdatePendingAwards();
                //}
            //}
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
                    //if (_pendingAwards != null && _pendingAwards.Count != 0)
                    //{
                    //    _pendingAwards.Clear();
                    //    UpdatePendingAwards();
                    //}

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
            lock (_pendingAwardsLock)
            {
                if (_progressiveRecovery)
                {
                    ProcessProgressiveLevels(evt.LinkedProgressiveLevels);

                    return;
                }

                //if (_pendingAwards.Count > 0)
                //{
                //    foreach (var (poolName, amountInPennies) in _pendingAwards)
                //    {
                //        if (!_progressives.ContainsKey(poolName) || amountInPennies == 0)
                //        {
                //            continue;
                //        }

                //        foreach (var level in _progressives[poolName])
                //        {
                //            var levelName = LevelName(level);
                //            if (!levelName.Equals(evt.LinkedProgressiveLevels.First().LevelName))
                //            {
                //                continue;
                //            }

                //            AwardJackpotLevel(amountInPennies, levelName, poolName);
                //            return;
                //        }
                //    }
                //}

                _currentLinkedProgressiveLevelsHit = evt.LinkedProgressiveLevels;
            }
        }

        private void UpdateProgressiveMeter()
        {
            if (!string.IsNullOrEmpty(_updateProgressiveMeter)&&
                _progressives.TryGetValue(_updateProgressiveMeter, out var progressiveInfos))
            {
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
            foreach (var progressiveInfos in _progressives)
            {
                //var value = _attributes.Get(progressiveInfos.Key, 0);
                var value = 0;

                if (value > 0)
                {
                    foreach (var progressive in progressiveInfos.Value)
                    {
                        UpdateLinkedProgressiveLevels(
                            progressive.ProgId,
                            progressive.LevelId,
                            value
                        );
                    }
                }
            }
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
                .Any(l => l.LevelName.Equals(linkedLevel.LevelName)))
            {
                _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                    new[] { linkedLevel },
                    ProtocolNames.Bingo);
            }

            //_logger.LogInfo(
            //    $"Updated linked progressive level: ProtocolName={linkedLevel.ProtocolName} ProgressiveGroupId={linkedLevel.ProgressiveGroupId} LevelName={linkedLevel.LevelName} LevelId={linkedLevel.LevelId} Amount={linkedLevel.Amount} ClaimStatus={linkedLevel.ClaimStatus} CurrentErrorStatus={linkedLevel.CurrentErrorStatus} Expiration={linkedLevel.Expiration}");

            return linkedLevel;
        }

        private string GetPoolName(string levelName)
        {
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
            foreach (var progressive in _progressives)
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
            {
                if(progressive.Value.Any(i => LevelName(i).Equals(levelName)))
                {
                    return progressive.Key;
                }
            }

            return string.Empty;
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