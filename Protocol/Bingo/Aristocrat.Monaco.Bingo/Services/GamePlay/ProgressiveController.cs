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
        private readonly IGameHistory _gameHistory;
        private readonly IProgressiveClaimService _progressiveClaimService;
        private readonly IProgressiveAwardService _progressiveAwardService;
        private readonly IPropertiesManager _propertiesManager;

        private readonly ConcurrentDictionary<string, IList<ProgressiveInfo>> _progressives = new();
        private readonly IList<ProgressiveInfo> _activeProgressiveInfos = new List<ProgressiveInfo>();
        private readonly object _pendingAwardsLock = new();
        private readonly IList<(string poolName, long progressiveLevelId, long amountInPennies, int awardId)> _pendingAwards = new List<(string, long, long, int)>();
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveController" /> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus" />.</param>
        /// <param name="gameProvider"><see cref="IGameProvider" />.</param>
        /// <param name="protocolLinkedProgressiveAdapter">.</param>
        /// <param name="gameHistory"><see cref="IGameHistory" />.</param>
        /// <param name="multiProtocolEventBusRegistry"><see cref="IProtocolProgressiveEventsRegistry" />.</param>
        /// <param name="progressiveClaimService"><see cref="IProgressiveClaimService" />.</param>
        /// <param name="propertiesManager"><see cref="IPropertiesManager" />.</param>
        public ProgressiveController(
            IEventBus eventBus,
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IGameHistory gameHistory,
            IProtocolProgressiveEventsRegistry multiProtocolEventBusRegistry,
            IProgressiveClaimService progressiveClaimService,
            IProgressiveAwardService progressiveAwardService,
            IPropertiesManager propertiesManager
            )
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _multiProtocolEventBusRegistry = multiProtocolEventBusRegistry ?? throw new ArgumentNullException(nameof(multiProtocolEventBusRegistry));
            _progressiveClaimService = progressiveClaimService ?? throw new ArgumentNullException(nameof(progressiveClaimService));
            _progressiveAwardService = progressiveAwardService ?? throw new ArgumentNullException(nameof(progressiveAwardService));
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
            // TODO
        }

        /// <inheritdoc />
        public void Configure()
        {
            Logger.Debug("ProgressiveController Configure");

            _progressives.Clear();
            _activeProgressiveInfos.Clear();

            // TODO handle if there are pending awards

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

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<HostDisconnectedEvent>(this, Handle);
            _eventBus.Subscribe<ProgressiveHostOfflineEvent>(this, Handle);
            _multiProtocolEventBusRegistry.SubscribeProgressiveEvent<LinkedProgressiveHitEvent>(ProtocolNames.Bingo, this);
            _eventBus.Subscribe<PendingLinkedProgressivesHitEvent>(this, Handle);
            _eventBus.Subscribe<PaytablesInstalledEvent>(this, Handle);
        }

        private void Handle(PaytablesInstalledEvent evt)
        {
            Configure();
        }

        private void Handle(HostDisconnectedEvent evt)
        {
            // TODO will need to deal with bingo host going offline
        }

        private void Handle(ProgressiveHostOfflineEvent evt)
        {
            // TODO will need to deal with progressive host going offline
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

                    // TODO copied code does not allow for multiple pending awards with the same pool name. For Bingo we allow hitting the same progressive multiple times. How to handle?
                    var poolName = GetPoolName(level.LevelName);
                    if (!string.IsNullOrEmpty(poolName) &&
                        (!_pendingAwards.Any(a => a.poolName.Equals(poolName, StringComparison.OrdinalIgnoreCase))))
                    {
                        _pendingAwards.Add((poolName, response.Result.ProgressiveLevelId, response.Result.ProgressiveWinAmount, response.Result.ProgressiveAwardId));
                    }

                    // TODO will need to persist the _pending award value in persistent storage
                }
            }
        }

        private void Handle(LinkedProgressiveHitEvent evt)
        {
            // TODO this is handled during the hit, claim, award sequence
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