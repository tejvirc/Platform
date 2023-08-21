namespace Aristocrat.Monaco.Asp.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Extensions;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <inheritdoc cref="IProgressiveManager" />
    public class ProgressiveManager : IProgressiveManager
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IProtocolProgressiveEventsRegistry _multiProtocolEventBusRegistry;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IPersistentStorageManager _storage;
        private readonly IPerLevelMeterProvider _perLevelMeterProvider;

        private LinkedProgressiveHitEvent _activeJackpotHit;

        private bool _disposed;

        /// <inheritdoc />
        public IReadOnlyDictionary<int, ILinkProgressiveLevel> Levels { get; private set; }

        /// <inheritdoc />
        public event EventHandler<OnNotificationEventArgs> OnNotificationEvent;

        public ProgressiveManager(
            IEventBus eventBus,
            IProtocolProgressiveEventsRegistry multiProtocolEventBusRegistry,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPersistentStorageManager persistentStorageManager,
            IPerLevelMeterProvider perLevelMeterProvider)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _multiProtocolEventBusRegistry = multiProtocolEventBusRegistry ?? throw new ArgumentNullException(nameof(multiProtocolEventBusRegistry));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _storage = persistentStorageManager ?? throw new ArgumentNullException(nameof(persistentStorageManager));
            _perLevelMeterProvider = perLevelMeterProvider ?? throw new ArgumentNullException(nameof(perLevelMeterProvider));

            LoadProgressiveLevels();
            SubscribeToEvents();
        }

        /// <inheritdoc />
        public void UpdateLinkJackpotHitAmountWon(int levelId, long amount)
        {
            if (_activeJackpotHit?.Level.LevelId != levelId) return;

            using (var transaction = _storage.ScopedTransaction())
            {
                _perLevelMeterProvider.SetValue(levelId, ProgressivePerLevelMeters.LinkJackpotHitAmountWon, amount);
                _perLevelMeterProvider.IncrementValue(levelId, ProgressivePerLevelMeters.TotalJackpotAmount, amount);

                var level = _activeJackpotHit.LinkedProgressiveLevels.First();
                ClaimJackpot(level.LevelId, level.LevelName, amount);
                transaction.Complete();
            }

            RaiseNotificationEvent(levelId, new List<string> { ProgressivePerLevelMeters.JackpotHitStatus, ProgressivePerLevelMeters.TotalJackpotAmount, ProgressivePerLevelMeters.JackpotResetCounter });

            _activeJackpotHit = null;
        }

        /// <inheritdoc />
        public void UpdateProgressiveJackpotAmountUpdate(int levelId, long amount)
        {
            if (!(_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .SingleOrDefault(w => FilterByLevelGroupProtocol(levelId, w)) is LinkedProgressiveLevel existingLevel)) return;

            var updatedLevel = new LinkedProgressiveLevel
            {
                LevelId = existingLevel.LevelId,
                Expiration = existingLevel.Expiration,
                ProgressiveGroupId = existingLevel.ProgressiveGroupId,
                ProtocolName = existingLevel.ProtocolName,
                Amount = amount,
                ClaimStatus = existingLevel.ClaimStatus,
                WagerCredits = existingLevel.WagerCredits,
                CurrentErrorStatus = ProgressiveErrors.None,
            };

            _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevelsAsync(new List<IViewableLinkedProgressiveLevel> { updatedLevel }, ProtocolNames.DACOM);
        }

        /// <inheritdoc />
        public void HandleProgressiveEvent<T>(T @event)
        {
            if (!(@event is LinkedProgressiveHitEvent linkProgressiveEvent)) return;

            var progressiveLevel = linkProgressiveEvent.Level;
            var levelId = progressiveLevel.LevelId;

            if (_activeJackpotHit != null)
            {
                Log.Error($"Received {nameof(LinkedProgressiveHitEvent)} when an existing active jackpot has not completed processing.");
                throw new DuplicateJackpotHitForLevelException($"{nameof(LinkedProgressiveHitEvent)} received for link progressive level when there is already an active jackpot");
            }

            _activeJackpotHit = linkProgressiveEvent;
            if (_perLevelMeterProvider.GetValue(levelId, ProgressivePerLevelMeters.JackpotHitStatus) != 1)
            {
                _perLevelMeterProvider.SetValue(levelId, ProgressivePerLevelMeters.JackpotHitStatus, 1);
                _perLevelMeterProvider.IncrementValue(levelId, ProgressivePerLevelMeters.TotalJackpotHitCount, 1);
            }

            RaiseNotificationEvent(levelId, new List<string> { ProgressivePerLevelMeters.JackpotHitStatus, ProgressivePerLevelMeters.TotalJackpotHitCount });

            Log.Info($"Hit received for {progressiveLevel.LevelName} jackpot of {progressiveLevel.CurrentValue} for level {levelId}");
        }

        private static bool FilterByLevelGroupProtocol(int levelId, IViewableLinkedProgressiveLevel progressiveLevel) =>
            progressiveLevel.LevelId == levelId && progressiveLevel.ProgressiveGroupId == ProgressiveConstants.ProgressiveGroupId && progressiveLevel.ProtocolName == ProtocolNames.DACOM;

        private void LoadProgressiveLevels()
        {
            var activeProgressiveLevels = _protocolLinkedProgressiveAdapter.ViewConfiguredProgressiveLevels()
                .Where(w => w.LevelType == ProgressiveLevelType.LP);

            var levels = Enumerable.Range(0, ProgressiveConstants.LinkProgressiveMaxLevels)
                .Select(s => new { LevelId = s, ActiveLevels = activeProgressiveLevels.Where(w => w.LevelId == s).ToList().AsReadOnly() })
                .Select
                (
                    w => new
                    {
                        w.LevelId,
                        Level = (ILinkProgressiveLevel)(w.ActiveLevels.Any()
                            ? new LinkProgressiveLevel(w.LevelId, w.ActiveLevels.First().LevelName, _perLevelMeterProvider.GetValue, GetLevelAmountCallback)
                            : null)
                    }
                )
                .Where(w => w.Level != null)
                .ToDictionary(pair => pair.LevelId, pair => pair.Level);

            Levels = new ReadOnlyDictionary<int, ILinkProgressiveLevel>(levels);

            var levelsAndNames = Levels.Values
                .Select(s => new { s.LevelId, s.Name })
                .ToList();

            if (!levelsAndNames.Any()) return;

            var output = string.Join(", ", levelsAndNames.Select(s => $"{s.Name} (level {s.LevelId + 1})"));
            Log.Info($"Progressive levels {output} configured");

            //We'll also notify the jackpot datasources that the progressive manager has initialised and pass them their initial data
            var jackpotNumberAndControllerIds = Levels.ToDictionary(
                pair => pair.Key,
                pair => new JackpotNumberAndControllerIdState
                {
                    LevelId = pair.Key,
                    JackpotNumber = Levels[pair.Key].CurrentJackpotNumber,
                    JackpotControllerIdByteOne = Levels[pair.Key].JackpotControllerIdByteOne,
                    JackpotControllerIdByteTwo = Levels[pair.Key].JackpotControllerIdByteTwo,
                    JackpotControllerIdByteThree = Levels[pair.Key].JackpotControllerIdByteThree
                });

            _eventBus.Publish(new ProgressiveManageUpdatedEvent(jackpotNumberAndControllerIds));
        }

        public Dictionary<int, LevelAmountUpdateState> GetJackpotAmountsPerLevel()
        {
            return Levels.ToDictionary(
                pair => pair.Key,
                pair => new LevelAmountUpdateState { Amount = GetLevelAmountCallback(pair.Key), Update = _perLevelMeterProvider.GetValue(pair.Key, ProgressivePerLevelMeters.JackpotAmountUpdate) == 1 });
        }

        public void UpdateProgressiveJackpotAmountUpdate(Dictionary<int, long> amounts)
        {
            var levels = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels();

            var updates = amounts
                .Select(levelAmount => new { Level = levels.SingleOrDefault(level => FilterByLevelGroupProtocol(levelAmount.Key, level)), Amount = levelAmount.Value })
                .Where(w => w.Level is LinkedProgressiveLevel)
                .Select(existingLevel => new LinkedProgressiveLevel
                {
                    LevelId = existingLevel.Level.LevelId,
                    Expiration = existingLevel.Level.Expiration,
                    ProgressiveGroupId = existingLevel.Level.ProgressiveGroupId,
                    ProtocolName = existingLevel.Level.ProtocolName,
                    Amount = existingLevel.Amount,
                    ClaimStatus = existingLevel.Level.ClaimStatus,
                    WagerCredits = existingLevel.Level.WagerCredits,
                    CurrentErrorStatus = ProgressiveErrors.None,
                })
                .ToList();

            using (var transaction = _storage.ScopedTransaction())
            {
                //Update the game
                _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(updates, ProtocolNames.DACOM);

                //Update meters
                levels.Select(s => s.LevelId).ForEach(levelId => _perLevelMeterProvider.SetValue(levelId, ProgressivePerLevelMeters.JackpotAmountUpdate, amounts.ContainsKey(levelId) ? 1 : 0));

                transaction.Complete();
            }
        }

        private void SubscribeToEvents()
        {
            //This is published from the game and starts a jackpot claim cycle
            _multiProtocolEventBusRegistry.SubscribeProgressiveEvent<LinkedProgressiveHitEvent>(ProtocolNames.DACOM, this);

            //For when the DACOM device doesn't acknowledge the claim in time - listen for ButtonLogicalId.Button30 being pressed then remove lockup
            _eventBus.Subscribe<LinkedProgressiveClaimExpiredEvent>(this, _ => _eventBus.Subscribe<DownEvent>(this, HandleClaimTimeoutCleared));

            //Triggered when claim timeout has been cleared by operator - stop listening for ButtonLogicalId.Button30 to be pressed
            _eventBus.Subscribe<LinkedProgressiveClaimRefreshedEvent>(this, _ => _eventBus.Unsubscribe<DownEvent>(this));

            //Triggered when link progressive configuration has changed
            _eventBus.Subscribe<LinkProgressiveLevelConfigurationAppliedEvent>(this, _ => LoadProgressiveLevels());

            //Notifies progressive controller of money being added to jackpot pool (when player starts a game)
            _eventBus.Subscribe<LinkedProgressiveUpdatedEvent>(this, NotifyGameJackpotAmountUpdated);

            //Notifies datasource that progressive controller wants to update jackpot number and controller id for a progressive level
            _eventBus.Subscribe<JackpotNumberAndControllerIdUpdateEvent>(this, UpdateJackpotNumberAndControllerId);
        }

        private void UpdateJackpotNumberAndControllerId(JackpotNumberAndControllerIdUpdateEvent state)
        {
            using (var transaction = _storage.ScopedTransaction())
            {
                _perLevelMeterProvider.SetValue(state.LevelId, ProgressivePerLevelMeters.CurrentJackpotNumber, state.JackpotNumber);
                _perLevelMeterProvider.SetValue(state.LevelId, ProgressivePerLevelMeters.JackpotControllerId, state.JackpotControllerId);
                transaction.Complete();
            }
        }

        private long GetLevelAmountCallback(int levelId) =>
            _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()?.SingleOrDefault(f => FilterByLevelGroupProtocol(levelId, f))?.Amount ?? 0;

        private void NotifyGameJackpotAmountUpdated(LinkedProgressiveUpdatedEvent eventArgs) =>
            eventArgs.LinkedProgressiveLevels.ToList().ForEach(f => RaiseNotificationEvent(f.LevelId, ProgressivePerLevelMeters.JackpotAmountUpdate));

        private void HandleClaimTimeoutCleared(DownEvent theEvent)
        {
            try
            {
                if (_activeJackpotHit == null) return;
                if (theEvent.LogicalId != (int)ButtonLogicalId.Button30) return;

                //Reset (by claiming with zero amount) active jackpot
                var level = _activeJackpotHit.LinkedProgressiveLevels.First();

                using (var transaction = _storage.ScopedTransaction())
                {
                    ClaimJackpot(level.LevelId, level.LevelName, 0);
                    transaction.Complete();
                }

                RaiseNotificationEvent(level.LevelId, new List<string> { ProgressivePerLevelMeters.JackpotHitStatus, ProgressivePerLevelMeters.JackpotResetCounter });

                _activeJackpotHit = null;

                Log.Warn("Active jackpot has been cleared with no payout via operator action");
            }
            catch (Exception ex)
            {
                Log.Error("Exception thrown while trying to clear jackpot claim timeout", ex);
            }
        }

        private void ClaimJackpot(int levelId, string levelName, long amount)
        {
            Log.Info($"Claiming {levelName} jackpot of {amount} for level {levelId}...");

            _protocolLinkedProgressiveAdapter.ClaimLinkedProgressiveLevel(levelName, ProtocolNames.DACOM);
            _protocolLinkedProgressiveAdapter.AwardLinkedProgressiveLevel(levelName, amount, ProtocolNames.DACOM);

            Log.Info($"Claimed {levelName} jackpot of {amount} for level {levelId}");

            _perLevelMeterProvider.SetValue(levelId, ProgressivePerLevelMeters.JackpotHitStatus, 0);
            _perLevelMeterProvider.IncrementValue(levelId, ProgressivePerLevelMeters.JackpotResetCounter, 1);
        }

        private void RaiseNotificationEvent(int levelId, string memberName) => RaiseNotificationEvent(levelId, new List<string> { memberName });

        private void RaiseNotificationEvent(int levelId, IReadOnlyList<string> memberNames) => OnNotificationEvent?.Invoke(this, new OnNotificationEventArgs(new Dictionary<int, IReadOnlyList<string>> { { levelId, memberNames } }));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}