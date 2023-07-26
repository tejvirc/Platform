namespace Aristocrat.Monaco.TestProtocol
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Gaming.Contracts;
    using Gaming.Contracts.Events.OperatorMenu;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    public class TestProgressiveController : IDisposable, IProtocolProgressiveEventHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IProtocolProgressiveEventsRegistry _multiProtocolEventBusRegistry;
        private readonly IPersistentStorageManager _storage;

        private readonly ConcurrentDictionary<int, List<ProgressiveLevelAssignment>> _pools =
            new ConcurrentDictionary<int, List<ProgressiveLevelAssignment>>();

        private bool _disposed;

        public TestProgressiveController()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IGameProvider>(),
                ServiceManager.GetInstance().GetService<IProtocolLinkedProgressiveAdapter>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IProtocolProgressiveEventsRegistry>())
        {
        }

        public TestProgressiveController(
            IEventBus eventBus,
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPersistentStorageManager storage,
            IProtocolProgressiveEventsRegistry multiProtocolEventBusRegistry)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                                throw new ArgumentNullException(
                                                    nameof(protocolLinkedProgressiveAdapter));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            _multiProtocolEventBusRegistry = multiProtocolEventBusRegistry ??
                                             throw new ArgumentNullException(nameof(multiProtocolEventBusRegistry));

            SubscribeToEvents();
        }

        /// <inheritdoc />
        ~TestProgressiveController()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void HandleProgressiveEvent<T>(T @event)
        {
            Handle(@event as LinkedProgressiveHitEvent);
        }

        public void Configure()
        {
            _pools.Clear();

            try
            {
                var pools =
                    (from level in _protocolLinkedProgressiveAdapter.ViewProgressiveLevels()
                            .Where(
                                x => x.LevelType == ProgressiveLevelType.LP &&
                                     _gameProvider.GetGames().Where(g => g.EgmEnabled)
                                         .Any(g => g.VariationId == x.Variation || x.Variation.ToUpper() == "ALL"))
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
                            level.ResetValue)).ToList();

                    _protocolLinkedProgressiveAdapter.AssignLevelsToGame(
                        progressiveLevelAssignment,
                        ProtocolNames.Test);

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

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _multiProtocolEventBusRegistry.UnSubscribeProgressiveEvent<LinkedProgressiveHitEvent>(
                    ProtocolNames.Test,
                    this);
            }

            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _multiProtocolEventBusRegistry.SubscribeProgressiveEvent<LinkedProgressiveHitEvent>(
                ProtocolNames.Test,
                this);
            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, Handle);
            _eventBus.Subscribe<GameConfigurationSaveCompleteEvent>(this, _ => Configure());
        }

        private void Handle(PrimaryGameStartedEvent evt)
        {
            if (_pools.TryGetValue(evt.GameId, out var levels))
            {
                foreach (var level in levels)
                {
                    if (_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(
                        level.ProgressiveLevel.AssignedProgressiveId.AssignedProgressiveKey,
                        out var linkedLevel))
                    {
                        var denominationInCents = level.Denom.MillicentsToCents();
                        var newValue = linkedLevel.Amount +
                                       evt.Log.InitialWager / denominationInCents *
                                       (level.ProgressiveLevel.IncrementRate > 0
                                           ? level.ProgressiveLevel.IncrementRate.ToPercentage()
                                           : (decimal)0.25);

                        UpdateLinkedProgressiveLevels(
                            level.ProgressiveLevel.ProgressiveId,
                            level.ProgressiveLevel.LevelId,
                            (long)newValue);
                    }
                }
            }
        }

        private void Handle(LinkedProgressiveHitEvent evt)
        {
            ProcessProgressiveLevels(evt);
        }

        private void ProcessProgressiveLevels(LinkedProgressiveHitEvent evt)
        {
            foreach (var level in evt.LinkedProgressiveLevels)
            {
                if (level.ClaimStatus.Status == LinkedClaimState.Hit)
                {
                    using (var scope = _storage.ScopedTransaction())
                    {
                        Log.Debug($"AwardJackpot {level.LevelName} amount {level.Amount}");
                        _protocolLinkedProgressiveAdapter.ClaimLinkedProgressiveLevel(
                            level.LevelName,
                            ProtocolNames.Test);
                        _protocolLinkedProgressiveAdapter.AwardLinkedProgressiveLevel(
                            level.LevelName,
                            level.Amount,
                            ProtocolNames.Test);

                        scope.Complete();
                    }

                    if (_pools.TryGetValue(evt.Level.GameId, out var levels))
                    {
                        foreach (var levelUpdate in levels)
                        {
                            if (level.LevelName.Equals(
                                    levelUpdate.ProgressiveLevel.AssignedProgressiveId.AssignedProgressiveKey) &&
                                _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(
                                    levelUpdate.ProgressiveLevel.AssignedProgressiveId.AssignedProgressiveKey,
                                    out _))
                            {
                                UpdateLinkedProgressiveLevels(
                                    levelUpdate.ProgressiveLevel.ProgressiveId,
                                    levelUpdate.ProgressiveLevel.LevelId,
                                    levelUpdate.InitialValue.MillicentsToCents());
                            }
                        }
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

            if (!initialize || !_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Any(l => l.LevelName.Equals(linkedLevel.LevelName)))
            {
                _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                    new[] { linkedLevel },
                    ProtocolNames.Test);
            }

            Log.Debug(
                $"Updated linked progressive level: ProtocolName={linkedLevel.ProtocolName} ProgressiveGroupId={linkedLevel.ProgressiveGroupId} LevelName={linkedLevel.LevelName} LevelId={linkedLevel.LevelId} Amount={linkedLevel.Amount} ClaimStatus={linkedLevel.ClaimStatus} CurrentErrorStatus={linkedLevel.CurrentErrorStatus} Expiration={linkedLevel.Expiration}");

            return linkedLevel;
        }

        private static LinkedProgressiveLevel LinkedProgressiveLevel(int progId, int levelId, long valueInCents)
        {
            var linkedLevel = new LinkedProgressiveLevel
            {
                ProtocolName = ProtocolNames.Test,
                ProgressiveGroupId = progId,
                LevelId = levelId,
                Amount = valueInCents,
                Expiration = DateTime.UtcNow + TimeSpan.FromDays(365),
                CurrentErrorStatus = ProgressiveErrors.None
            };

            return linkedLevel;
        }
    }
}