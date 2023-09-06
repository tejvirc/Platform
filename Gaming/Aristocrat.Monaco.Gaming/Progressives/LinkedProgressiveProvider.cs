namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Timers;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <inheritdoc cref="ILinkedProgressiveProvider" />
    public sealed class LinkedProgressiveProvider : ILinkedProgressiveProvider, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, LinkedProgressiveLevel> _linkedProgressiveIndex;

        private readonly IEventBus _eventBus;
        private readonly IProgressiveBroadcastTimer _broadcastTimer;
        private readonly IProgressiveMeterManager _meters;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IPersistentBlock _saveBlock;
        private readonly IPersistentStorageManager _storage;
        private readonly string _saveKey;
        private readonly object _syncLock = new();

        private bool _disposed;

        public LinkedProgressiveProvider(
            IEventBus bus,
            IProgressiveBroadcastTimer broadcastTimer,
            IProgressiveMeterManager meters,
            IPersistenceProvider persistenceProvider,
            IPropertiesManager propertiesManager,
            IPersistentStorageManager storage)
        {
            _eventBus = bus ?? throw new ArgumentNullException(nameof(bus));
            _broadcastTimer = broadcastTimer ?? throw new ArgumentNullException(nameof(broadcastTimer));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            _saveKey = nameof(LinkedProgressiveProvider);

            _saveBlock = persistenceProvider?.GetOrCreateBlock(_saveKey, PersistenceLevel.Static) ??
                         throw new NullReferenceException(nameof(_saveBlock));

            _linkedProgressiveIndex =
                _saveBlock.GetOrCreateValue<Dictionary<string, LinkedProgressiveLevel>>(_saveKey) ??
                throw new NullReferenceException(nameof(_linkedProgressiveIndex));

            _broadcastTimer.Timer.Elapsed += CheckLevelExpiration;

            _eventBus.Subscribe<ProgressiveHitEvent>(this, Handle);
            _eventBus.Subscribe<ProgressiveCommitEvent>(this, Handle);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _eventBus?.UnsubscribeAll(this);

            _broadcastTimer.Timer.Stop();
            _broadcastTimer.Timer.Elapsed -= CheckLevelExpiration;
            _disposed = true;
        }

        /// <inheritdoc />
        public void AddLinkedProgressiveLevels(IReadOnlyCollection<IViewableLinkedProgressiveLevel> linkedProgressiveLevels)
        {
            CheckForNull(linkedProgressiveLevels);
            lock (_syncLock)
            {
                using var transaction = _storage.ScopedTransaction();
                InternalAddLinkedProgressiveLevels(linkedProgressiveLevels);
                Save();
                transaction.Complete();
            }
        }

        /// <inheritdoc />
        public void RemoveLinkedProgressiveLevels(IReadOnlyCollection<IViewableLinkedProgressiveLevel> linkedProgressiveLevels)
        {
            CheckForNull(linkedProgressiveLevels);

            lock (_syncLock)
            {
                using var transaction = _storage.ScopedTransaction();
                var removedLevels = new List<IViewableLinkedProgressiveLevel>();

                foreach (var level in linkedProgressiveLevels)
                {
                    if (LockedTryGetLinkedLevel(level.LevelName, out var removedLevel))
                    {
                        _linkedProgressiveIndex.Remove(level.LevelName);
                        removedLevels.Add(removedLevel);
                        Logger.Debug($"Linked Level Removed: {removedLevel}");
                    }
                }

                PostEventIfAny(
                    removedLevels,
                    () =>
                        _eventBus.Publish(new LinkedProgressiveRemovedEvent(removedLevels)));

                Save();
                transaction.Complete();
            }
        }

        /// <inheritdoc />
        public void UpdateLinkedProgressiveLevels(IReadOnlyCollection<IViewableLinkedProgressiveLevel> levelUpdates)
        {
            CheckForNull(levelUpdates);

            var levelsToAdd = new List<IViewableLinkedProgressiveLevel>();
            var levelsToUpdate = new List<IViewableLinkedProgressiveLevel>();


            lock (_syncLock)
            {
                using var transaction = _storage.ScopedTransaction();
                //split up levels that haven't been initially added
                foreach (var level in levelUpdates)
                {
                    if (_linkedProgressiveIndex.ContainsKey(level.LevelName))
                    {
                        levelsToUpdate.Add(level);
                    }
                    else
                    {
                        levelsToAdd.Add(level);
                    }
                }

                if (levelsToAdd.Any())
                {
                    InternalAddLinkedProgressiveLevels(levelsToAdd);
                }

                if (levelsToUpdate.Any())
                {
                    InternalUpdateLinkedProgressiveLevels(levelsToUpdate);
                }

                Save();
                transaction.Complete();
            }
        }

        private void InternalAddLinkedProgressiveLevels(IEnumerable<IViewableLinkedProgressiveLevel> linkedProgressiveLevels)
        {
            var addedLevels = new List<IViewableLinkedProgressiveLevel>();

            foreach (var level in linkedProgressiveLevels)
            {
                var levelToAdd = new LinkedProgressiveLevel
                {
                    ProtocolName = level.ProtocolName,
                    ProgressiveGroupId = level.ProgressiveGroupId,
                    LevelId = level.LevelId,
                    Amount = level.Amount,
                    Expiration = level.Expiration,
                    CurrentErrorStatus = ProgressiveErrors.None,
                    ClaimStatus = new LinkedProgressiveClaimStatus(),
                    ProgressiveValueSequence = level.ProgressiveValueSequence,
                    ProgressiveValueText = level.ProgressiveValueText,
                    FlavorType = level.FlavorType,
                    CommonLevelName = level.CommonLevelName,
                };

                _linkedProgressiveIndex.Add(level.LevelName, levelToAdd);
                addedLevels.Add(levelToAdd);
                Logger.Debug($"Linked Level Added: {levelToAdd}");
                
            }

            _meters.AddLinkedProgressives(addedLevels);

            PostEventIfAny(
                addedLevels,
                () =>
                    _eventBus.Publish(new LinkedProgressiveAddedEvent(addedLevels)));
        }

        private void InternalUpdateLinkedProgressiveLevels(IEnumerable<IViewableLinkedProgressiveLevel> levelsToUpdate)
        {
            var updatedLevels = new List<IViewableLinkedProgressiveLevel>();

            foreach (var linkedLevel in levelsToUpdate)
            {
                var level = _linkedProgressiveIndex[linkedLevel.LevelName];

                // Persisted level with no claim status only occurs if
                // an existing level is persisted with no claim status.
                // This can only happen when upgrading in development... once.
                level.ClaimStatus ??= new LinkedProgressiveClaimStatus();

                // Freeze level updates until the claim is processed.
                if (level.ClaimStatus.Status == LinkedClaimState.None)
                {
                    level.Amount = linkedLevel.Amount;
                }

                level.WagerCredits = linkedLevel.WagerCredits;
                level.Expiration = linkedLevel.Expiration.ToUniversalTime();
                level.ProgressiveValueSequence = linkedLevel.ProgressiveValueSequence;
                level.ProgressiveValueText = linkedLevel.ProgressiveValueText;
                level.FlavorType = level.FlavorType;

                updatedLevels.Add(level);
                Logger.Debug($"Linked Level Updated: {level}");
            }

            PostEventIfAny(
                updatedLevels,
                () =>
                    _eventBus.Publish(new LinkedProgressiveUpdatedEvent(updatedLevels)));
        }

        public async Task UpdateLinkedProgressiveLevelsAsync(
            IReadOnlyCollection<IViewableLinkedProgressiveLevel> levelUpdates)
        {
            await Task.Run(() => UpdateLinkedProgressiveLevels(levelUpdates));
        }

        /// <inheritdoc />
        public void ReportLinkDown(int progressiveGroupId)
        {
            ReportLinkDownBy(level =>
                level.ProgressiveGroupId == progressiveGroupId &&
                !level.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgressiveDisconnected));
        }

        /// <inheritdoc />
        public void ReportLinkDown(string protocolName)
        {
            Logger.Debug("ReportLinkDown");
            ReportLinkDownBy(level =>
                level.ProtocolName == protocolName &&
                !level.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgressiveDisconnected));
        }

        /// <inheritdoc />
        public void ReportLinkUp(int progressiveGroupId)
        {
            ReportLinkUpBy(level =>
                level.ProgressiveGroupId == progressiveGroupId &&
                level.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgressiveDisconnected));
        }

        /// <inheritdoc />
        public void ReportLinkUp(string protocolName)
        {
            ReportLinkUpBy(level =>
                level.ProtocolName == protocolName &&
                level.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgressiveDisconnected));
        }

        /// <inheritdoc />
        public void ClaimLinkedProgressiveLevel(string levelName)
        {
            if (!LockedTryGetLinkedLevel(levelName, out var hitLevel))
            {
                throw new InvalidOperationException($"Unable to find progressive level to claim: {levelName}");
            }

            if (hitLevel.ClaimStatus.Status != LinkedClaimState.Hit)
            {
                throw new InvalidOperationException($"Unable to award a level without a hit: {hitLevel}");
            }

            hitLevel.ClaimStatus.Status = LinkedClaimState.Claimed;

            Save();
        }

        /// <inheritdoc />
        public void AwardLinkedProgressiveLevel(string levelName, long winAmount = -1)
        {
            if (!LockedTryGetLinkedLevel(levelName, out var claimedLevel))
            {
                // TODO convert to lockup
                throw new InvalidOperationException($"Unable to find progressive to award: {levelName}"); 
            }

            if (claimedLevel.ClaimStatus.Status != LinkedClaimState.Claimed)
            {
                // TODO convert to lockup
                throw new InvalidOperationException($"Attempt to award an unclaimed level: {claimedLevel}"); 
            }

            claimedLevel.ClaimStatus.Status = LinkedClaimState.Awarded;

            Save();

            _eventBus.Publish(
                new LinkedProgressiveAwardedEvent(
                    claimedLevel.ClaimStatus.TransactionId,
                    winAmount != -1
                        ? winAmount.CentsToMillicents()
                        : claimedLevel.ClaimStatus.WinAmount.CentsToMillicents(),
                    string.Empty,
                    PayMethod.Any));
        }

        /// <inheritdoc />
        public async Task AwardLinkedProgressiveLevelAsync(string levelName)
        {
            await Task.Run(() => AwardLinkedProgressiveLevel(levelName));
        }

        /// <inheritdoc />
        public void Reset(string levelName, IViewableJackpotTransaction transaction)
        {
            lock (_syncLock)
            {
                if (!LockedTryGetLinkedLevel(levelName, out var level))
                {
                    throw new InvalidOperationException($"Cannot find level to resolve: {levelName}");
                }

                if (level.ClaimStatus.Status != LinkedClaimState.Awarded)
                {
                    throw new InvalidOperationException($"Unable to resolve a claim that hasn't been awarded: {level}");
                }

                level.ClaimStatus.Status = LinkedClaimState.None;
                level.ClaimStatus.HitTime = DateTime.MinValue;
                level.ClaimStatus.ExpiredTime = DateTime.MaxValue;
                level.ClaimStatus.TransactionId = 0;
                level.ClaimStatus.WinAmount = 0;
                Save();
                _eventBus.Publish(new LinkedProgressiveResetEvent(transaction.TransactionId));
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IViewableLinkedProgressiveLevel> ViewLinkedProgressiveLevels() => LockedGetLinkedLevels();

        /// <inheritdoc />
        public bool ViewLinkedProgressiveLevel(string levelName, out IViewableLinkedProgressiveLevel levelOut)
        {
            var lookupResult = LockedTryGetLinkedLevel(levelName, out var level);
            levelOut = level;
            return lookupResult;
        }

        /// <inheritdoc />
        public bool ViewLinkedProgressiveLevels(IEnumerable<string> levelNames, out IReadOnlyCollection<IViewableLinkedProgressiveLevel> levelsOut)
        {
            var result = true;
            var returnList = new List<IViewableLinkedProgressiveLevel>();

            foreach (var name in levelNames)
            {
                result &= ViewLinkedProgressiveLevel(name, out var level);

                if (result)
                {
                    returnList.Add(level);
                }
                else
                {
                    returnList.Clear();
                    break;
                }
            }

            levelsOut = returnList;
            return result;
        }

        /// <inheritdoc />
        public void ClaimAndAwardLinkedProgressiveLevel(string levelName, long winAmount = -1)
        {
            lock (_syncLock)
            {
                using var scope = _storage.ScopedTransaction();
                ClaimLinkedProgressiveLevel(levelName);
                AwardLinkedProgressiveLevel(levelName, winAmount);
            }
        }

        public string Name => nameof(LinkedProgressiveProvider);

        public ICollection<Type> ServiceTypes => new[] { typeof(ILinkedProgressiveProvider) };

        public void Initialize()
        {
            _broadcastTimer.Timer.Start();
        }

        private static void EvaluateClaimExpiration(
            DateTime currentTime,
            LinkedProgressiveLevel level,
            List<IViewableLinkedProgressiveLevel> expiredLevels,
            List<IViewableLinkedProgressiveLevel> refreshedLevels)
        {
            if (level.ClaimStatus.Status == LinkedClaimState.Hit &&
                currentTime > level.ClaimStatus.ExpiredTime.ToUniversalTime() &&
                !level.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgCommitTimeout))
            {
                level.CurrentErrorStatus |= ProgressiveErrors.ProgCommitTimeout;
                expiredLevels.Add(level);
                Logger.Debug($"Linked Level Claim Timed Out: {level}");
            }
            else if (level.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgCommitTimeout) &&
                     (currentTime <= level.ClaimStatus.ExpiredTime.ToUniversalTime() ||
                      level.ClaimStatus.Status != LinkedClaimState.Hit))
            {
                level.CurrentErrorStatus &= ~ProgressiveErrors.ProgCommitTimeout;
                refreshedLevels.Add(level);
                Logger.Debug($"Linked Level Claimed Time Out Cleared: {level}");
            }
        }

        private static void EvaluateExpiration(
            DateTime currentTime,
            LinkedProgressiveLevel level,
            List<IViewableLinkedProgressiveLevel> expiredLevels,
            List<IViewableLinkedProgressiveLevel> refreshedLevels)
        {
            if (currentTime > level.Expiration.ToUniversalTime() &&
                !level.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgressiveUpdateTimeout))
            {
                level.CurrentErrorStatus |= ProgressiveErrors.ProgressiveUpdateTimeout;
                expiredLevels.Add(level);
                Logger.Debug($"Linked Level Expired: {level}");
            }
            else if (currentTime <= level.Expiration.ToUniversalTime() &&
                     level.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgressiveUpdateTimeout))
            {
                level.CurrentErrorStatus &= ~ProgressiveErrors.ProgressiveUpdateTimeout;
                refreshedLevels.Add(level);
                Logger.Debug($"Linked Level Refreshed: {level}");
            }
        }

        private static void PostEventIfAny(IList<IViewableLinkedProgressiveLevel> levels, Action postEvent)
        {
            if (levels.Any())
            {
                postEvent();
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void CheckForNull(IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            if (levels == null)
            {
                throw new ArgumentNullException(nameof(levels));
            }
        }

        private void CheckLevelExpiration(object sender, ElapsedEventArgs e)
        {
            var currentTime = DateTime.UtcNow;
            var allLevels = LockedGetLinkedLevels();
            var expiredLevels = new List<IViewableLinkedProgressiveLevel>();
            var refreshedLevels = new List<IViewableLinkedProgressiveLevel>();
            var expiredClaims = new List<IViewableLinkedProgressiveLevel>();
            var refreshedClaims = new List<IViewableLinkedProgressiveLevel>();
            var existingExpiredLevels = new List<IViewableLinkedProgressiveLevel>();

            foreach (var level in allLevels)
            {
                EvaluateExpiration(currentTime, level, expiredLevels, refreshedLevels);
                EvaluateClaimExpiration(currentTime, level, expiredClaims, refreshedClaims);

                if (level.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgressiveUpdateTimeout))
                {
                    existingExpiredLevels.Add(level);
                }
            }

            PostEventIfAny(
                expiredLevels,
                () =>
                    _eventBus.Publish(new LinkedProgressiveExpiredEvent(expiredLevels, existingExpiredLevels)));

            PostEventIfAny(
                refreshedLevels,
                () =>
                    _eventBus.Publish(new LinkedProgressiveRefreshedEvent(refreshedLevels)));

            PostEventIfAny(
                expiredClaims,
                () => _eventBus.Publish(new LinkedProgressiveClaimExpiredEvent(expiredClaims)));

            PostEventIfAny(
                refreshedClaims,
                () => _eventBus.Publish(new LinkedProgressiveClaimRefreshedEvent(refreshedClaims)));
        }

        private void Save()
        {
            lock (_syncLock)
            {
                using var transaction = _saveBlock.Transaction();
                transaction.SetValue(_saveKey, _linkedProgressiveIndex);
                transaction.Commit();
            }
        }

        private void ReportLinkDownBy(Func<LinkedProgressiveLevel, bool> filter)
        {
            var linkDownLevels = new List<IViewableLinkedProgressiveLevel>();
            var linkedLevels = LockedGetLinkedLevels();

            foreach (var linkedProgressiveLevel in linkedLevels.Where(filter))
            {
                linkedProgressiveLevel.CurrentErrorStatus |= ProgressiveErrors.ProgressiveDisconnected;
                linkDownLevels.Add(linkedProgressiveLevel);
                Logger.Debug($"Linked Level Disconnected: {linkedProgressiveLevel}");
            }

            PostEventIfAny(
                linkDownLevels,
                () =>
                    _eventBus.Publish(new LinkedProgressiveDisconnectedEvent(linkDownLevels)));
        }

        private void ReportLinkUpBy(Func<LinkedProgressiveLevel, bool> filter)
        {
            var linkUpLevels = new List<IViewableLinkedProgressiveLevel>();
            var linkedLevels = LockedGetLinkedLevels();

            foreach (var linkedLevel in linkedLevels.Where(filter))
            {
                linkedLevel.CurrentErrorStatus &= ~ProgressiveErrors.ProgressiveDisconnected;
                linkUpLevels.Add(linkedLevel);
                Logger.Debug($"Linked Level Connected: {linkedLevel}");
            }

            PostEventIfAny(
                linkUpLevels,
                () =>
                    _eventBus.Publish(new LinkedProgressiveConnectedEvent(linkUpLevels)));
        }

        private void Handle(ProgressiveHitEvent theEvent)
        {
            var assignedLevelId = theEvent.Level.AssignedProgressiveId;

            if (assignedLevelId.AssignedProgressiveType != AssignableProgressiveType.Linked)
            {
                return;
            }

            ProcessHit(theEvent.Level, theEvent.Jackpot, theEvent.IsRecovery);
        }

        private void ProcessHit(
            IViewableProgressiveLevel progressiveLevel,
            JackpotTransaction transaction,
            bool isRecovery = false)
        {
            var levelName = progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey;

            if (!LockedTryGetLinkedLevel(levelName, out var level))
            {
                throw new InvalidOperationException($"Unable to find progressive level to process hit: {levelName}");
            }

            if (isRecovery && (level.ClaimStatus.Status == LinkedClaimState.Claimed ||
                               level.ClaimStatus.Status == LinkedClaimState.Awarded))
            {
                Logger.Debug("Attempting to award a win for something that was already claimed during recovery");
                return;
            }

            // In recovery cases the Linked claim state can be "hit". 
            if (level.ClaimStatus.Status != LinkedClaimState.None && level.ClaimStatus.Status != LinkedClaimState.Hit)
            {
                throw new InvalidOperationException($"Unable to process hit on a level with existing claim: {level}");
            }

            var commitTimeoutMs = _propertiesManager.GetValue(
                GamingConstants.ProgressiveCommitTimeoutMs,
                GamingConstants.DefaultProgressiveCommitTimeoutMs);

            level.ClaimStatus.Status = LinkedClaimState.Hit;
            level.ClaimStatus.TransactionId = transaction.TransactionId;
            level.ClaimStatus.WinAmount = transaction.ValueAmount.MillicentsToCents();
            level.ClaimStatus.HitTime = transaction.TransactionDateTime;
            level.ClaimStatus.ExpiredTime = DateTime.UtcNow.AddMilliseconds(commitTimeoutMs);
            Save();

            _eventBus.Publish(new LinkedProgressiveHitEvent(
                progressiveLevel, new List<IViewableLinkedProgressiveLevel> { level }, transaction));
        }

        private void Handle(ProgressiveCommitEvent theEvent)
        {
            var assignedLevelId = theEvent.Level.AssignedProgressiveId;

            if (assignedLevelId.AssignedProgressiveType != AssignableProgressiveType.Linked)
            {
                return;
            }

            ProcessCommit(theEvent.Level, theEvent.Jackpot);
        }

        private void ProcessCommit(IViewableProgressiveLevel progressiveLevel, JackpotTransaction transaction)
        {
            var levelName = progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey;

            if (!LockedTryGetLinkedLevel(levelName, out var level))
            {
                throw new InvalidOperationException($"Unable to find progressive level to process commit: {levelName}");
            }

            _eventBus.Publish(new LinkedProgressiveCommitEvent(
                progressiveLevel, new List<IViewableLinkedProgressiveLevel> { level }, transaction));
        }

        private List<LinkedProgressiveLevel> LockedGetLinkedLevels()
        {
            lock (_syncLock)
            {
                return _linkedProgressiveIndex.Values.ToList();
            }
        }

        private bool LockedTryGetLinkedLevel(string levelName, out LinkedProgressiveLevel level)
        {
            lock (_syncLock)
            {
                return _linkedProgressiveIndex.TryGetValue(levelName, out level);
            }
        }
    }
}