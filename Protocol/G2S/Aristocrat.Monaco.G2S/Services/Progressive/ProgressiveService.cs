namespace Aristocrat.Monaco.G2S.Services.Progressive
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Timers;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Transactions;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.G2S;
    using Aristocrat.Monaco.G2S.Data.Model;
    using Aristocrat.Monaco.Gaming.Contracts.Events.OperatorMenu;
    using Aristocrat.Monaco.Gaming.Contracts.Meters;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Common.Events;
    using DisableProvider;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Handlers;
    using Handlers.Progressive;
    using Kernel;
    using log4net;
    using Newtonsoft.Json;

    public partial class ProgressiveService : IProgressiveService, IService, IDisposable, IProtocolProgressiveEventHandler
    {
        private const int DefaultNoProgInfoTimeout = 30000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IGameProvider _gameProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IPersistentStorageManager _storage;
        private readonly IProgressiveMeterManager _progressiveMeters;
        private readonly IGameHistory _gameHistory;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IG2SDisableProvider _disableProvider;
        private readonly IProtocolProgressiveEventsRegistry _protocolProgressiveEventsRegistry;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveStatus> _commandBuilder;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveHit> _progressiveHitBuilder;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveCommit> _progressiveCommitBuilder;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveStatus> _progressiveStatusBuilder;
        private readonly ConcurrentDictionary<string, IList<ProgressiveInfo>> _progressives = new ConcurrentDictionary<string, IList<ProgressiveInfo>>();
        private readonly object _pendingAwardsLock = new object();
        private readonly bool _g2sProgressivesEnabled;

        private bool _disposed;
        private Timer _progressiveValueUpdateTimer;
        private Timer _progressiveHostOfflineTimer;
        private IList<(string poolName, long amountInPennies)> _pendingAwards;
        private bool _progressiveRecovery;
        private IEnumerable<IViewableLinkedProgressiveLevel> _currentLinkedProgressiveLevelsHit;
        private IHostControl _progressiveHost = null;

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
            IProgressiveMeterManager progressiveMeters,
            IGameHistory gameHistory,
            ITransactionHistory transactionHistory,
            IUnitOfWorkFactory unitOfWorkFactory,
            IG2SDisableProvider disableProvider,
            ICommandBuilder<IProgressiveDevice, progressiveStatus> statusCommandBuilder,
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
            _progressiveMeters = progressiveMeters ?? throw new ArgumentNullException(nameof(progressiveMeters));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _disableProvider = disableProvider ?? throw new ArgumentNullException(nameof(disableProvider));
            _commandBuilder = statusCommandBuilder ?? throw new ArgumentNullException(nameof(statusCommandBuilder));
            _progressiveStatusBuilder = progressiveStatusBuilder ?? throw new ArgumentNullException(nameof(progressiveStatusBuilder));
            _progressiveHitBuilder = progressiveHitBuilder ?? throw new ArgumentNullException(nameof(progressiveHitBuilder));
            _progressiveCommitBuilder = progressiveCommitBuilder ?? throw new ArgumentNullException(nameof(progressiveCommitBuilder));

            var propertyProvider = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _g2sProgressivesEnabled = (bool)propertyProvider.GetProperty(G2S.Constants.G2SProgressivesEnabled, false);
            if (_g2sProgressivesEnabled)
            {
                SubscribeEvents();

                //Setup the progressive host monitoring timers. They will be started once we actually connect. 
                _progressiveValueUpdateTimer = new Timer(DefaultNoProgInfoTimeout);
                _progressiveValueUpdateTimer.Elapsed += ProgressiveValueUpdateTimerElapsed;
                _lastProgressiveUpdateTime = DateTime.UtcNow;
                _progressiveHostOfflineTimer = new Timer();
                _progressiveHostOfflineTimer.Elapsed += ProgressiveHostOfflineTimerElapsed;
                _disableProvider.Disable(SystemDisablePriority.Immediate, G2SDisableStates.CommsOffline);

                Configure();
            }
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IProgressiveService), typeof(IProtocolProgressiveIdProvider) };

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Initializing the G2S VoucherDataService.");
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
                    _protocolProgressiveEventsRegistry.UnSubscribeProgressiveEvent<ProgressiveCommitEvent>(
                        ProtocolNames.G2S, this);
                    _protocolProgressiveEventsRegistry.UnSubscribeProgressiveEvent<ProgressiveCommitAckEvent>(
                        ProtocolNames.G2S, this);
                    _protocolProgressiveEventsRegistry.UnSubscribeProgressiveEvent<ProgressiveHitEvent>(
                        ProtocolNames.G2S, this);
                    _protocolProgressiveEventsRegistry.UnSubscribeProgressiveEvent<LinkedProgressiveHitEvent>(
                        ProtocolNames.G2S, this);

                    _progressiveHostOfflineTimer?.Stop();
                    _progressiveHostOfflineTimer?.Dispose();
                    _progressiveValueUpdateTimer?.Stop();
                    _progressiveValueUpdateTimer?.Dispose();
                }
            }

            _progressiveHostOfflineTimer = null;
            _progressiveValueUpdateTimer = null;

            _disposed = true;
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<HostUnreachableEvent>(this, CommunicationsStateChanged);
            _eventBus.Subscribe<TransportDownEvent>(this, CommunicationsStateChanged);
            _eventBus.Subscribe<TransportUpEvent>(this, CommunicationsStateChanged);
            _eventBus.Subscribe<ProgressiveWagerCommittedEvent>(this, OnProgressiveWagerCommitted);
            _eventBus.Subscribe<GameConfigurationSaveCompleteEvent>(this, _ => Configure());
            _protocolProgressiveEventsRegistry.SubscribeProgressiveEvent<ProgressiveCommitEvent>(
                ProtocolNames.G2S, this);
            _protocolProgressiveEventsRegistry.SubscribeProgressiveEvent<ProgressiveCommitAckEvent>(
                ProtocolNames.G2S, this);
            _protocolProgressiveEventsRegistry.SubscribeProgressiveEvent<ProgressiveHitEvent>(
                ProtocolNames.G2S, this);
            _protocolProgressiveEventsRegistry.SubscribeProgressiveEvent<LinkedProgressiveHitEvent>(
                ProtocolNames.G2S,
                this);
        }

        private void OnTransportUp()
        {
            var progressiveHostInfos = GetProgressiveHostInfo();

            if (progressiveHostInfos == null || progressiveHostInfos.All(i => i == null))
            {
                Logger.Info("ProgressiveHostInfo is not found");
                return;
            }

            var devices = _egm.GetDevices<IProgressiveDevice>().ToList();
            foreach (var progressiveHostInfo in progressiveHostInfos)
            {
                var progressiveDevice = devices.Where(d => d.ProgressiveId == progressiveHostInfo.progressiveLevel.FirstOrDefault().progId).FirstOrDefault();
                if (progressiveDevice == null)
                {
                    continue;
                }

                var matchedProgressives = progressiveHostInfo.progressiveLevel.Where(
                progressiveLevel => progressiveLevel.progId.Equals(progressiveDevice.Id));

                if (!matchedProgressives.Any())
                {
                    progressiveDevice.Enabled = false;
                    return;
                }

                var levelProvider = ServiceManager.GetInstance().GetService<IProgressiveLevelProvider>();
                var progLevels = levelProvider.GetProgressiveLevels().Where(l => l.ProgressiveId == progressiveDevice.ProgressiveId && l.DeviceId != 0);

                var levelsMatched = true;
                try
                {
                    levelsMatched = progLevels.Count() == matchedProgressives.Count() &&
                        progLevels.Select(l => LevelIds.GetVertexProgressiveLevelId(l.GameId, l.ProgressiveId, l.LevelId)).OrderBy(n => n)
                        .SequenceEqual(matchedProgressives.Select(l => l.levelId).OrderBy(n => n));
                }
                catch (KeyNotFoundException)
                {
                    levelsMatched = false;
                }

                if (!levelsMatched)
                {
                    _disableProvider.Disable(SystemDisablePriority.Immediate, G2SDisableStates.LevelMismatch);
                    return;
                }

                _disableProvider.Enable(G2SDisableStates.LevelMismatch);
            }


            //ensure the progressive value update timer is set to correct monitor timeout
            var updateTimeout = devices.Select(device => device.NoProgressiveInfo).Prepend(int.MaxValue).Min();

            if (updateTimeout is <= 0 or int.MaxValue)
            {
                updateTimeout = DefaultNoProgInfoTimeout;
            }

            _progressiveValueUpdateTimer.Interval = updateTimeout;
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

        private setProgressiveWin ProgressiveHit(int deviceId, long transactionId)
        {
            var timeout = TimeSpan.MaxValue;
            var currentUtcNow = DateTime.UtcNow;
            var progressiveDevice = _egm.GetDevice<IProgressiveDevice>(VertexDeviceIds[deviceId]);
            var command = new progressiveHit();
            var progressiveLog = new progressiveLog();
            command.transactionId = transactionId;
            _progressiveHitBuilder.Build(progressiveDevice, command);

            var task = progressiveDevice.ProgressiveHit(command, t_progStates.G2S_progHit, progressiveLog, timeout);
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

        private progressiveCommitAck ProgressiveCommit(int deviceId, long transactionId)
        {
            var timeout = TimeSpan.MaxValue;
            var currentUtcNow = DateTime.UtcNow;
            var progressiveDevice = _egm.GetDevice<IProgressiveDevice>(VertexDeviceIds[deviceId]);
            var command = new progressiveCommit();
            var progressiveLog = new progressiveLog();
            command.transactionId = transactionId;

            _progressiveCommitBuilder.Build(progressiveDevice, command);

            var task = progressiveDevice.ProgressiveCommit(command, progressiveLog, t_progStates.G2S_progCommit);
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

        private void SetGamePlayDeviceState(bool state, IProgressiveDevice device)
        {
            var gamePlayDevices = _egm.GetDevices<IGamePlayDevice>();
            var levelProvider = ServiceManager.GetInstance().GetService<IProgressiveLevelProvider>();
            var gamePlayDeviceIds = levelProvider.GetProgressiveLevels()
                                            .Where(l => l.ProgressiveId == device.ProgressiveId && l.DeviceId == device.Id)
                                            .Select(l => l.GameId);

            foreach (var gamePlayDeviceId in gamePlayDeviceIds)
            {
                var gamePlayDevice = _egm.GetDevice<IGamePlayDevice>(gamePlayDeviceId);
                if (gamePlayDevice != null)
                {
                    gamePlayDevice.Enabled = state;
                }
            }
        }

        private void OnProgressiveWagerCommitted(ProgressiveWagerCommittedEvent theEvent)
        {
            var level = theEvent.Levels.FirstOrDefault();

            if (level == null)
            {
                throw new InvalidOperationException($"There are no progressive levels");
            }

            var device = _egm.GetDevice<IProgressiveDevice>(VertexDeviceIds[level.DeviceId]);
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
                                simpleMeter = GetProgressiveLevelMeters(level.DeviceId,
                                    ProgressiveMeters.WageredAmount,
                                    ProgressiveMeters.PlayedCount).ToArray()
                            }
                        }
                }
            };

            _eventLift.Report(device, EventCode.G2S_PGE101, device.DeviceList(status), new meterList { meterInfo = meters.ToArray() });
        }

        private void CommunicationsStateChanged(IEvent theEvent)
        {
            if (theEvent.GetType() == typeof(TransportUpEvent))
            {
                _disableProvider.Enable(G2SDisableStates.CommsOffline);
                _disableProvider.Disable(SystemDisablePriority.Immediate, G2SDisableStates.ProgressiveValueNotReceived);
                _progressiveHostOfflineTimer.Start();
                Task.Run(OnTransportUp);
            }
            else
            {
                _disableProvider.Disable(SystemDisablePriority.Immediate, G2SDisableStates.CommsOffline);
                _progressiveHostOfflineTimer.Stop();
            }
        }

        private void HandleEvent(ProgressiveCommitEvent evt)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(VertexDeviceIds[evt.Jackpot.DeviceId]);

            if (evt.Jackpot.State != ProgressiveState.Committed)
            {
                return;
            }

            var progressiveCommitAck = ProgressiveCommit(VertexDeviceIds[evt.Level.DeviceId], evt.Jackpot.TransactionId);

            if (progressiveCommitAck == null)
            {
                Logger.Error("progressiveCommit command not ACK'd by progressive host.");
            }

            EventReport(device, evt.Jackpot, EventCode.G2S_PGE104);
        }

        private void HandleEvent(ProgressiveCommitAckEvent evt)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(VertexDeviceIds[evt.Jackpot.DeviceId]);

            if (evt.Jackpot.State != ProgressiveState.Committed)
            {
                return;
            }

            EventReport(device, evt.Jackpot, EventCode.G2S_PGE105);
        }

        private void HandleEvent(ProgressiveHitEvent evt)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(VertexDeviceIds[evt.Jackpot.DeviceId]);

            if (evt.Jackpot.State != ProgressiveState.Hit)
            {
                return;
            }

            EventReport(device, evt.Jackpot, EventCode.G2S_PGE102);
        }

        private void HandleEvent(LinkedProgressiveHitEvent evt)
        {
            var setProgressiveWin = Helpers.RetryForever(() => ProgressiveHit(VertexDeviceIds[evt.Level.DeviceId], evt.TransactionId));

            if (setProgressiveWin != null)
            {
                var progInfo = GetProgInfo(evt.Level.ProgressiveId, evt.Level.LevelId);
                AwardJackpot(progInfo.PoolName, setProgressiveWin.progWinAmt.MillicentsToCents());
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
                    foreach (var (poolName, amountInPennies) in _pendingAwards)
                    {
                        if (!_progressives.ContainsKey(poolName) || amountInPennies == 0)
                        {
                            continue;
                        }

                        foreach (var level in _progressives[poolName])
                        {
                            var levelName = LevelName(level);
                            if (!levelName.Equals(evt.LinkedProgressiveLevels.First().LevelName, StringComparison.Ordinal))
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
            EventHandlerDevice.EventReport(
                device.PrefixedDeviceClass(),
                device.Id,
                eventCode,
                transactionId: log.TransactionId,
                transactionList: new transactionList
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
                case ProgressiveCommitEvent commitEvent:
                    HandleEvent(commitEvent);
                    break;
                case ProgressiveCommitAckEvent commitAckEvent:
                    HandleEvent(commitAckEvent);
                    break;
                case ProgressiveHitEvent hitEvent:
                    HandleEvent(hitEvent);
                    break;
                case LinkedProgressiveHitEvent hitEvent:
                    HandleEvent(hitEvent);
                    break;
            }
        }

        private readonly ConcurrentDictionary<int, List<ProgressiveLevelAssignment>> _pools =
            new ConcurrentDictionary<int, List<ProgressiveLevelAssignment>>();

        /// <summary>
        /// Configures the LinkedProgressive list that is stored through the ProtocolLinkedProgressiveAdapter.
        /// </summary>
        public void Configure(bool clearLinkedLevels = false)
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

            try
            {
                var enabledGames = _gameProvider.GetGames().Where(g => g.EgmEnabled);
                var enabledLinkedLevels = _protocolLinkedProgressiveAdapter.ViewProgressiveLevels()
                            .Where(
                                x => x.LevelType == ProgressiveLevelType.LP &&
                                     enabledGames.Any(g => (g.VariationId == x.Variation || x.Variation.ToUpper() == "ALL") && x.GameId == g.Id));

                var pools =
                    (from level in enabledLinkedLevels
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

                    var linkedLevel = UpdateLinkedProgressiveLevels(
                        pool.Key.ProgId,
                        pool.Key.LevelId,
                        resetValue.MillicentsToCents(),
                        true);

                    var progressiveLevelAssignment = pool.Select(
                        level => new ProgressiveLevelAssignment(
                            game,
                            level.Denomination.First(),
                            level,
                            new AssignableProgressiveId(
                                AssignableProgressiveType.Linked,
                                linkedLevel.LevelName),
                            //level.AssignedProgressiveId,
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
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var pendingJackpots = unitOfWork.Repository<PendingJackpotAwards>().Queryable().SingleOrDefault() ??
                                      new PendingJackpotAwards();

                pendingJackpots.Awards = JsonConvert.SerializeObject(_pendingAwards);

                unitOfWork.Repository<PendingJackpotAwards>().AddOrUpdate(pendingJackpots);

                unitOfWork.SaveChanges();
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