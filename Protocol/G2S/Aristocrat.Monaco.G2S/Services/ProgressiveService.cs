namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Common.Events;
    using Data.Model;
    using DisableProvider;
    using G2S;
    using Gaming.Contracts;
    using Gaming.Contracts.Events.OperatorMenu;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Handlers;
    using Handlers.Progressive;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Newtonsoft.Json;

    public class ProgressiveService : IProgressiveService, IService, IDisposable, IProtocolProgressiveEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IGameProvider _gameProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IPersistentStorageManager _storage;
        private readonly IGameHistory _gameHistory;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IG2SDisableProvider _disableProvider;
        private readonly IProgressiveLevelManager _progressiveLevelManager;
        private readonly IProtocolProgressiveEventsRegistry _protocolProgressiveEventsRegistry;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveHit> _progressiveHitBuilder;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveCommit> _progressiveCommitBuilder;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveStatus> _progressiveStatusBuilder;
        private readonly ConcurrentDictionary<string, IList<ProgressiveInfo>> _progressives = new ConcurrentDictionary<string, IList<ProgressiveInfo>>();
        private readonly object _pendingAwardsLock = new object();
        private readonly bool _g2sProgressivesEnabled;

        private bool _disposed;
        private IList<(string poolName, long amountInPennies)> _pendingAwards;
        private bool _progressiveRecovery;
        private IEnumerable<IViewableLinkedProgressiveLevel> _currentLinkedProgressiveLevelsHit;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveService" /> class.
        /// </summary>
        public ProgressiveService(
            IG2SEgm egm,
            IEventLift eventLift,
            IEventBus eventBus,
            IGameProvider gameProvider,
            IProtocolProgressiveEventsRegistry protocolProgressiveEventSubscriber,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPersistentStorageManager storage,
            IGameHistory gameHistory,
            ITransactionHistory transactionHistory,
            IUnitOfWorkFactory unitOfWorkFactory,
            IG2SDisableProvider disableProvider,
            IPropertiesManager propertiesManager,
            IProgressiveLevelManager progressiveLevelManager,
            ICommandBuilder<IProgressiveDevice, progressiveStatus> progressiveStatusBuilder,
            ICommandBuilder<IProgressiveDevice, progressiveHit> progressiveHitBuilder,
            ICommandBuilder<IProgressiveDevice, progressiveCommit> progressiveCommitBuilder
            )
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolProgressiveEventsRegistry = protocolProgressiveEventSubscriber ??
                                                  throw new ArgumentNullException(
                                                      nameof(protocolProgressiveEventSubscriber));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                                throw new ArgumentNullException(
                                                    nameof(protocolLinkedProgressiveAdapter));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _disableProvider = disableProvider ?? throw new ArgumentNullException(nameof(disableProvider));
            _progressiveLevelManager = progressiveLevelManager ?? throw new ArgumentNullException(nameof(progressiveLevelManager));
            _progressiveStatusBuilder = progressiveStatusBuilder ?? throw new ArgumentNullException(nameof(progressiveStatusBuilder));
            _progressiveHitBuilder = progressiveHitBuilder ?? throw new ArgumentNullException(nameof(progressiveHitBuilder));
            _progressiveCommitBuilder = progressiveCommitBuilder ?? throw new ArgumentNullException(nameof(progressiveCommitBuilder));

            _g2sProgressivesEnabled = (bool)propertiesManager.GetProperty(G2S.Constants.G2SProgressivesEnabled, false);
            if (!_g2sProgressivesEnabled)
            {
                return;
            }

            SubscribeEvents();
            Configure();

            if (_egm.GetDevices<IProgressiveDevice>().Any())
            {
                //start in a disabled state until communications are established with the progressive host
                _disableProvider.Disable(
                    SystemDisablePriority.Immediate,
                    G2SDisableStates.ProgressiveHostCommsOffline);
            }
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IProgressiveService) };

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Initializing the G2S ProgressiveService.");
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                if (_g2sProgressivesEnabled)
                {
                    _protocolProgressiveEventsRegistry.UnSubscribeProgressiveEvent<LinkedProgressiveHitEvent>(
                        ProtocolNames.G2S, this);
                    _protocolProgressiveEventsRegistry.UnSubscribeProgressiveEvent<LinkedProgressiveCommitEvent>(
                        ProtocolNames.G2S, this);
                    _protocolProgressiveEventsRegistry.UnSubscribeProgressiveEvent<ProgressiveCommitAckEvent>(
                        ProtocolNames.G2S, this);
                }
            }

            _disposed = true;
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<CommunicationsStateChangedEvent>(this, OnCommunicationsStateChanged);
            _eventBus.Subscribe<GameConfigurationSaveCompleteEvent>(this, Configure);
            _eventBus.Subscribe<ProgressiveWagerCommittedEvent>(this, OnProgressiveWagerCommitted);
            _protocolProgressiveEventsRegistry.SubscribeProgressiveEvent<LinkedProgressiveHitEvent>(
                ProtocolNames.G2S, this);
            _protocolProgressiveEventsRegistry.SubscribeProgressiveEvent<LinkedProgressiveCommitEvent>(
                ProtocolNames.G2S, this);
            _protocolProgressiveEventsRegistry.SubscribeProgressiveEvent<ProgressiveCommitAckEvent>(
                ProtocolNames.G2S, this);
        }

        private void OnCommsOnline()
        {
            var progressiveHostInfos = GetProgressiveHostInfo();

            if (progressiveHostInfos == null || progressiveHostInfos.All(i => i == null))
            {
                Logger.Info("ProgressiveHostInfo is not found");
                return;
            }

            var devices = _egm.GetDevices<IProgressiveDevice>().ToList();

            if (devices.Count != progressiveHostInfos.Count)
            {
                _disableProvider.Disable(SystemDisablePriority.Immediate, G2SDisableStates.ProgressiveLevelsMismatch);
                return;
            }

            var linkedLevels = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Where(ll => ll.ProtocolName == ProtocolNames.G2S)
                .GroupBy(ll => ll.ProgressiveGroupId)
                .ToDictionary(ll => ll.Key, ll => ll.ToList());

            foreach (var progressiveHostInfo in progressiveHostInfos)
            {
                var progressiveDevice = devices.FirstOrDefault(d => d.ProgressiveId == progressiveHostInfo.progressiveLevel.FirstOrDefault().progId);
                if (progressiveDevice == null)
                {
                    continue;
                }

                var matchedProgressives = progressiveHostInfo.progressiveLevel
                    .Where(progressiveLevel => progressiveLevel.progId.Equals(progressiveDevice.Id)).ToList();

                if (!matchedProgressives.Any())
                {
                    progressiveDevice.Enabled = false;
                    return;
                }

                var progLevels = linkedLevels[progressiveDevice.ProgressiveId];
                var levelsMatched = true;
                try
                {
                    levelsMatched = progLevels.Count() == matchedProgressives.Count() &&
                        progLevels.Select(ll => ll.LevelId).OrderBy(n => n)
                        .SequenceEqual(matchedProgressives.Select(l => l.levelId).OrderBy(n => n));
                }
                catch (KeyNotFoundException)
                {
                    levelsMatched = false;
                }

                if (!levelsMatched)
                {
                    _disableProvider.Disable(SystemDisablePriority.Immediate, G2SDisableStates.ProgressiveLevelsMismatch);
                    return;
                }

                _disableProvider.Enable(G2SDisableStates.ProgressiveLevelsMismatch);
            }
        }

        private List<progressiveHostInfo> GetProgressiveHostInfo()
        {
            var infoToReturn = new List<progressiveHostInfo>();
            var devices = _egm.GetDevices<IProgressiveDevice>();

            foreach (var progressiveDevice in devices)
            {
                var timeout = TimeSpan.MaxValue;
                var currentUtcNow = DateTime.UtcNow;

                var command = new getProgressiveHostInfo();

                var progressiveHostInfo = progressiveDevice.GetProgressiveHostInfo(command, timeout);
                if (progressiveHostInfo == null)
                {
                    if (DateTime.UtcNow - currentUtcNow > timeout)
                    {
                        Logger.Info($"Command was unsuccessful, posting {EventCode.G2S_PGE106} event");
                    }
                    continue;
                }

                if (!progressiveHostInfo.IsValid())
                {
                    Logger.Info("Received ProgressiveHostInfo is invalid");
                    continue;
                }

                infoToReturn.Add(progressiveHostInfo);
            }

            return infoToReturn;
        }

        private setProgressiveWin ProgressiveHit(IProgressiveDevice device, long transactionId)
        {
            var timeout = TimeSpan.MaxValue;
            var currentUtcNow = DateTime.UtcNow;
            var command = new progressiveHit();
            var progressiveLog = new progressiveLog();
            command.transactionId = transactionId;
            _progressiveHitBuilder.Build(device, command);

            var task = device.ProgressiveHit(command, t_progStates.G2S_progHit, progressiveLog, timeout);
            task.Wait();
            var setProgWin = task.Result;
            if (setProgWin == null)
            {
                if (DateTime.UtcNow - currentUtcNow > timeout)
                {
                    Logger.Info($"Command was unsuccessful, posting {EventCode.G2S_PGE106} event");
                }

                throw new Exception();
            }

            if (!setProgWin.IsValid())
            {
                Logger.Info("Received SetProgressiveWin is invalid");

                throw new Exception();
            }

            var transaction = _transactionHistory.RecallTransaction<JackpotTransaction>(command.transactionId);
            transaction.WinSequence = setProgWin.progWinSeq;
            _transactionHistory.UpdateTransaction(transaction);

            return setProgWin;
        }

        private progressiveCommitAck ProgressiveCommit(IProgressiveDevice device, long transactionId)
        {
            var command = new progressiveCommit();
            var progressiveLog = new progressiveLog();
            command.transactionId = transactionId;

            _progressiveCommitBuilder.Build(device, command);

            var task = device.ProgressiveCommit(command, progressiveLog, t_progStates.G2S_progCommit);
            task.Wait();
            var progCommitAck = task.Result;
            if (progCommitAck == null)
            {
                Logger.Error("progressiveCommit command failed.");
                return null;
            }

            if (!progCommitAck.IsValid())
            {
                Logger.Info("Received progressiveCommitAck is invalid");
                return null;
            }

            return progCommitAck;
        }

        private void OnProgressiveWagerCommitted(ProgressiveWagerCommittedEvent theEvent)
        {
            var level = theEvent.Levels.FirstOrDefault(l => l.LevelType == ProgressiveLevelType.LP);
            if (level == null)
            {
                //If no levels are linked, this is a SAP only game. No need to report meters
                return;
            }

            _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(level.AssignedProgressiveId.AssignedProgressiveKey, out var linkedLevel);
            if (linkedLevel == null)
            {
                throw new InvalidOperationException($"Invalid Assigned LinkedProgressiveLevel on level. Level.DeviceId = {level.DeviceId}, AssignedProgressiveKey={level.AssignedProgressiveId.AssignedProgressiveKey}");
            }

            var device = _egm.GetDevice<IProgressiveDevice>(linkedLevel.ProgressiveGroupId);
            var status = new progressiveStatus();
            _progressiveStatusBuilder.Build(device, status);

            var meters = new List<meterInfo>
            {
                new meterInfo
                {
                    meterDateTime = DateTime.UtcNow,
                    meterInfoType = MeterInfoType.Event,
                    deviceMeters =
                        new[]
                        {
                            new deviceMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = device.Id,
                                simpleMeter = _progressiveLevelManager.GetProgressiveLevelMeters(linkedLevel.LevelName,
                                    ProgressiveMeters.LinkedProgressiveWageredAmount,
                                    ProgressiveMeters.LinkedProgressivePlayedCount).ToArray()
                            }
                        }
                }
            };

            _eventLift.Report(device, EventCode.G2S_PGE101, device.DeviceList(status), new meterList { meterInfo = meters.ToArray() });
        }

        private void OnCommunicationsStateChanged(CommunicationsStateChangedEvent evt)
        {
            var host = _egm.GetHostById(evt.HostId);
            if (!host.IsProgressiveHost) return;

            if (evt.Online)
            {
                _disableProvider.Enable(G2SDisableStates.ProgressiveHostCommsOffline);
                Task.Run(OnCommsOnline);
            }
            else
            {
                _disableProvider.Disable(SystemDisablePriority.Immediate, G2SDisableStates.ProgressiveHostCommsOffline);
            }

        }

        private void HandleEvent(LinkedProgressiveCommitEvent evt)
        {
            var linkedLevel = evt.LinkedProgressiveLevels.First();
            var device = _egm.GetDevice<IProgressiveDevice>(linkedLevel.ProgressiveGroupId);

            if (evt.Jackpot.State != ProgressiveState.Committed)
            {
                return;
            }

            EventReport(device, evt.Jackpot, EventCode.G2S_PGE104);

            var progressiveCommitAck = ProgressiveCommit(device, evt.Jackpot.TransactionId);

            if (progressiveCommitAck == null)
            {
                Logger.Error("progressiveCommit command not ACK'd by progressive host.");
            }

            _eventBus.Publish(new ProgressiveCommitAckEvent(evt.Jackpot));
        }

        private void HandleEvent(ProgressiveCommitAckEvent evt)
        {
            _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(
                evt.Jackpot.AssignedProgressiveKey,
                out var linkedLevel);
            var device = _egm.GetDevice<IProgressiveDevice>(linkedLevel.ProgressiveGroupId);

            if (evt.Jackpot.State != ProgressiveState.Committed)
            {
                return;
            }

            EventReport(device, evt.Jackpot, EventCode.G2S_PGE105);
        }

        private void HandleEvent(LinkedProgressiveHitEvent evt)
        {
            var linkedLevel = evt.LinkedProgressiveLevels.First();
            var device = _egm.GetDevice<IProgressiveDevice>(linkedLevel.ProgressiveGroupId);

            EventReport(device, evt.Jackpot, EventCode.G2S_PGE102);

            var setProgressiveWin = Helpers.RetryForever(() => ProgressiveHit(device, evt.Jackpot.TransactionId));

            EventReport(device, evt.Jackpot, EventCode.G2S_PGE103);

            if (setProgressiveWin != null)
            {
                var progInfo = GetProgInfo(linkedLevel.ProgressiveGroupId, linkedLevel.LevelId);
                AwardJackpot(progInfo.PoolName, setProgressiveWin.progWinAmt.MillicentsToCents());
            }

            lock (_pendingAwardsLock)
            {
                if (_progressiveRecovery)
                {
                    ProcessProgressiveLevels(evt.LinkedProgressiveLevels);
                    _progressiveRecovery = false;
                    return;
                }

                if (_pendingAwards.Count > 0)
                {
                    foreach (var (poolName, amountInPennies) in _pendingAwards)
                    {
                        if (!_progressives.ContainsKey(poolName) || amountInPennies == 0)
                        {
                            continue;
                        }

                        foreach (var level in _progressives[poolName])
                        {
                            var levelName = LevelName(level);
                            if (!levelName.Equals(linkedLevel.LevelName, StringComparison.Ordinal))
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

        private void EventReport(IProgressiveDevice device, JackpotTransaction log, string eventCode)
        {
            _eventLift.Report(
                device,
                eventCode,
                log.TransactionId,
                new transactionList
                {
                    transactionInfo = new[]
                    {
                        new transactionInfo
                        {
                            deviceId = device.Id,
                            deviceClass = device.PrefixedDeviceClass(),
                            Item = log.ToProgressiveLog(_gameProvider)
                        }
                    }
                });
        }

        public void HandleProgressiveEvent<T>(T data)
        {
            switch (data)
            {
                case LinkedProgressiveHitEvent hitEvent:
                    HandleEvent(hitEvent);
                    break;
                case LinkedProgressiveCommitEvent commitEvent:
                    HandleEvent(commitEvent);
                    break;
                case ProgressiveCommitAckEvent commitAckEvent:
                    HandleEvent(commitAckEvent);
                    break;
            }
        }

        private readonly ConcurrentDictionary<int, List<ProgressiveLevelAssignment>> _pools =
            new ConcurrentDictionary<int, List<ProgressiveLevelAssignment>>();

        /// <summary>
        /// Configures the LinkedProgressive list that is stored through the ProtocolLinkedProgressiveAdapter.
        /// </summary>
        public void Configure(GameConfigurationSaveCompleteEvent _ = null)
        {
            _progressives.Clear();
            if (_pendingAwards == null)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var pendingJackpots = unitOfWork.Repository<PendingJackpotAwards>().Queryable().SingleOrDefault();
                    _pendingAwards = pendingJackpots?.Awards == null
                        ? new List<(string, long)>()
                        : JsonConvert.DeserializeObject<IList<(string, long)>>(pendingJackpots.Awards);
                }

                CheckProgressiveRecovery();
            }
            _pools.Clear();

            var propertiesManager = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
            var vertexLevelIds = (Dictionary<int, (int linkedGroupId, int linkedLevelId)>)propertiesManager.GetProperty(GamingConstants.ProgressiveConfiguredLinkedLevelIds,
                new Dictionary<int, (int linkedGroupId, int linkedLevelId)>());



            try
            {
                var enabledGames = _gameProvider.GetGames().Where(g => g.EgmEnabled).ToList();
                var enabledLinkedLevels = _protocolLinkedProgressiveAdapter.ViewProgressiveLevels()
                            .Where(l => FilterEnabledLinkedLevels(l, enabledGames));

                var pools =
                    (from level in enabledLinkedLevels
                     join linkConfig in vertexLevelIds on level.DeviceId equals linkConfig.Key
                     group level by new
                     {
                         level.GameId,
                         PackName = level.ProgressivePackName,
                         ProgId = linkConfig.Value.linkedGroupId,
                         LevelId = linkConfig.Value.linkedLevelId,
                         level.LevelName,
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

                    var linkedLevel = _progressiveLevelManager.UpdateLinkedProgressiveLevels(
                        pool.Key.ProgId,
                        pool.Key.LevelId,
                        resetValue.MillicentsToCents(),
                        0,
                        string.Empty,
                        true);

                    var progressiveLevelAssignment = pool.Select(
                        level => new ProgressiveLevelAssignment(
                            game,
                            level.Denomination.First(),
                            level,
                            new AssignableProgressiveId(
                                AssignableProgressiveType.Linked,
                                linkedLevel.LevelName),
                            level.ResetValue)).ToList();

                    var poolName = $"{pool.Key.PackName}_{resetValue.MillicentsToDollars()}_{pool.Key.LevelName}";
                    var valueAttributeName = $"ins{poolName}_Value";
                    var messageAttributeName = $"ins{poolName}_Message";

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
                        var list = new List<ProgressiveInfo>();
                        list.Add(progressive);
                        _progressives.TryAdd(valueAttributeName, list);
                    }

                    _protocolLinkedProgressiveAdapter.AssignLevelsToGame(
                        progressiveLevelAssignment,
                        ProtocolNames.G2S);

                    if (_pools.TryGetValue(game.Id, out var levels))
                    {
                        levels.AddRange(progressiveLevelAssignment);
                    }
                    else
                    {
                        _pools[game.Id] = progressiveLevelAssignment;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static bool FilterEnabledLinkedLevels(IViewableProgressiveLevel level, IEnumerable<IGameDetail> enabledGames)
        {
            if (level.LevelType != ProgressiveLevelType.LP) return false;

            //Check the game is enabled, and the level matches ALL variations, or it matches the enabled game's variation
            return enabledGames.Any(
                g => level.GameId == g.Id &&
                     (level.Variation.ToUpper() == "ALL" ||
                      level.Variation.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                          .Any(c => string.Equals(c.TrimStart('0'), g.VariationId.TrimStart('0')))));
        }

        private static string LevelName(ProgressiveInfo info)
        {
            return $"{ProtocolNames.G2S}, Level Id: {info.LevelId}, Progressive Group Id: {info.ProgId}";
        }

        private void AwardJackpot(string poolName, long amountInPennies)
        {
            if (_gameHistory?.CurrentLog.PlayState == PlayState.Idle)
            {
                return;
            }

            if (!string.IsNullOrEmpty(poolName) && amountInPennies > 0)
            {
                lock (_pendingAwardsLock)
                {
                    var valueAttributeName = $"ins{poolName}_Value";
                    var level = _currentLinkedProgressiveLevelsHit?.FirstOrDefault();

                    // add to pending if current level is null or if another level is hit
                    if (level == null || !_pendingAwards.Any(
                        a => a.poolName.Equals(valueAttributeName, StringComparison.Ordinal)))
                    {
                        var replaceAward = _pendingAwards.FirstOrDefault(
                        a => a.poolName.Equals(valueAttributeName, StringComparison.Ordinal) && a.amountInPennies.Equals(0));

                        if (replaceAward != default((string, long)))
                        {
                            _pendingAwards.Remove(replaceAward);
                        }

                        _pendingAwards.Add((valueAttributeName, amountInPennies));

                        UpdatePendingAwards();

                        return;
                    }

                    AwardJackpotLevel(amountInPennies, level.LevelName, valueAttributeName);
                }
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

            if (_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(levelName, out var level) &&
                level.ClaimStatus.Status == LinkedClaimState.Hit)
            {
                using (var scope = _storage.ScopedTransaction())
                {
                    _protocolLinkedProgressiveAdapter.ClaimLinkedProgressiveLevel(
                        levelName,
                        ProtocolNames.G2S);
                    _protocolLinkedProgressiveAdapter.AwardLinkedProgressiveLevel(
                        levelName,
                        amountInPennies,
                        ProtocolNames.G2S);

                    scope.Complete();
                }
            }

            lock (_pendingAwardsLock)
            {
                var award = _pendingAwards.FirstOrDefault(
                    a => a.poolName.Equals(attributePropertyName, StringComparison.Ordinal) &&
                         (a.amountInPennies == amountInPennies || a.amountInPennies == 0));
                if (award != default((string, long)))
                {
                    _pendingAwards.Remove(award);

                    UpdatePendingAwards();
                }
                _currentLinkedProgressiveLevelsHit = null;
            }
        }

        private long JackpotAmountInPennies()
        {
            var result = _currentLinkedProgressiveLevelsHit?.FirstOrDefault()?.Amount ?? 0;

            if (result == 0)
            {
                var claimedJackpotTotal =
                    _gameHistory?.CurrentLog.Jackpots.Sum(j => j.WinAmount.MillicentsToCents()) ?? 0;

                var totalJackpots = _gameHistory?.CurrentLog.Outcomes.LastOrDefault();

                if (totalJackpots != null && _pendingAwards.Count >= 1)
                {
                    return totalJackpots.Value.MillicentsToCents() - claimedJackpotTotal;
                }
            }

            return result;
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

        private void CheckProgressiveRecovery()
        {
            if (_gameHistory?.CurrentLog?.PlayState != PlayState.Idle &&
                _gameHistory?.CurrentLog?.Outcomes.Count() > 1 &&
                (JackpotAmountInPennies() != 0 || AllJackpotsClaimed()))
            {
                _progressiveRecovery = true;
            }
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

        private ProgressiveInfo GetProgInfo(int progId, int levelId)
        {
            var returnValue = _progressives.Values.SelectMany(list => list)
                                       .Single(info => info.ProgId == progId && info.LevelId == levelId);

            return returnValue;
        }
    }
}