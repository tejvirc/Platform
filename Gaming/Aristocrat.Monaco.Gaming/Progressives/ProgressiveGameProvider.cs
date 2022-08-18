namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Runtime;

    public class ProgressiveGameProvider : IProgressiveGameProvider, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IProgressiveLevelProvider _levelProvider;
        private readonly IGameStorage _gameStorage;
        private readonly ITransactionHistory _transactions;
        private readonly IGameHistory _gameHistory;
        private readonly IRuntime _runtime;
        private readonly IEventBus _bus;
        private readonly IProgressiveCalculatorFactory _calculatorFactory;
        private readonly IProgressiveMeterManager _meters;
        private readonly IPersistentStorageManager _storageManager;
        private readonly ILinkedProgressiveProvider _linkedProgressives;
        private readonly ISharedSapProvider _sharedSap;
        private readonly ISapProvider _standaloneProgressives;
        private readonly IPropertiesManager _propertiesManager;

        private readonly List<long> _pendingTransactions = new List<long>();
        private readonly object _sync = new object();

        private IReadOnlyCollection<ProgressiveLevel> _activeLevels = new List<ProgressiveLevel>();
        private IDictionary<int, long> _pendingContributions = new Dictionary<int, long>();
        private int _gameId = -1;
        private long _denomination;
        private string _packName;

        private bool _disposed;

        public ProgressiveGameProvider(
            IProgressiveLevelProvider levelProvider,
            IGameStorage gameStorage,
            ITransactionHistory transactions,
            IGameHistory gameHistory,
            IRuntime runtime,
            IEventBus bus,
            IProgressiveCalculatorFactory calculatorFactory,
            IProgressiveMeterManager meters,
            IPersistentStorageManager storageManager,
            ILinkedProgressiveProvider linkedProgressiveProvider,
            ISharedSapProvider sharedSapProvider,
            ISapProvider standaloneProgressives,
            IPropertiesManager propertiesManager)
        {
            _levelProvider = levelProvider ?? throw new ArgumentNullException(nameof(levelProvider));
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _calculatorFactory = calculatorFactory ?? throw new ArgumentNullException(nameof(calculatorFactory));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _linkedProgressives = linkedProgressiveProvider ??
                                  throw new ArgumentNullException(nameof(linkedProgressiveProvider));
            _sharedSap = sharedSapProvider ?? throw new ArgumentNullException(nameof(sharedSapProvider));
            _standaloneProgressives =
                standaloneProgressives ?? throw new ArgumentNullException(nameof(standaloneProgressives));

            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            _bus.Subscribe<GameProcessExitedEvent>(this, _ => Reset());
            _bus.Subscribe<LinkedProgressiveAwardedEvent>(this, Handle);
            _bus.Subscribe<SharedSapAwardedEvent>(this, Handle);
            _bus.Subscribe<SapAwardedEvent>(this, Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<IViewableProgressiveLevel> ActivateProgressiveLevels(
            string packName,
            int gameId,
            long denomination,
            string betOption)
        {
            var totalLevel = _levelProvider.GetProgressiveLevels(packName, gameId, denomination);
            _activeLevels = totalLevel.Where(
                l => (l.BetOption.IsNullOrEmpty() || l.BetOption == betOption) &&
                     (l.LevelType == ProgressiveLevelType.Sap ||
                      !l.AssignedProgressiveId.AssignedProgressiveKey.IsNullOrEmpty() &&
                      l.AssignedProgressiveId.AssignedProgressiveType != AssignableProgressiveType.None)).ToList();

            foreach (var level in _activeLevels)
            {
                level.CurrentState = ProgressiveLevelState.Active;

                if (GetLinkedLevel(level.AssignedProgressiveId, out var linkedLevel))
                {
                    level.CurrentValue = linkedLevel.Amount.CentsToMillicents();
                }
                else if (GetSharedSapLevel(level.AssignedProgressiveId, out var cSapLevel))
                {
                    level.CurrentValue = cSapLevel.CurrentValue;
                    level.Residual = cSapLevel.Residual;
                    level.HiddenIncrementRate = cSapLevel.HiddenIncrementRate;
                    level.HiddenValue = cSapLevel.HiddenValue;
                    level.Overflow = cSapLevel.Overflow;
                    level.OverflowTotal = cSapLevel.OverflowTotal;
                    level.ResetValue = cSapLevel.ResetValue;
                }
            }

            _gameId = gameId;
            _denomination = denomination;
            _packName = packName;
            _pendingContributions.Clear();

            // This may ultimately not be needed once configuration is supported, but this is currently required to allocate the device Ids
            _levelProvider.UpdateProgressiveLevels(packName, _gameId, _denomination, _activeLevels);

            Logger.Debug(
                $"Activated {_activeLevels.Count} out of {totalLevel.Count} progressive levels for {packName} ({_gameId} @ {_denomination})");

            _bus.Publish(new ProgressivesActivatedEvent());

            return _activeLevels;
        }

        public IEnumerable<IViewableProgressiveLevel> DeactivateProgressiveLevels(string packName)
        {
            var result = new List<IViewableProgressiveLevel>(_activeLevels);

            Logger.Debug($"Deactivating progressive levels for {packName} ({_gameId} @ {_denomination})");

            Reset();

            return result;
        }

        public IEnumerable<Jackpot> GetJackpotSnapshot(string packName)
        {
            return GetUniqueLevelIDs().Select(level => new Jackpot(level.DeviceId, level.LevelId, level.CurrentValue));
        }

        public IEnumerable<IViewableProgressiveLevel> GetActiveProgressiveLevels()
        {
            return new List<IViewableProgressiveLevel>(_activeLevels);
        }

        public IEnumerable<IViewableLinkedProgressiveLevel> GetActiveLinkedProgressiveLevels()
        {
            var activeLinkedLevels = new List<IViewableLinkedProgressiveLevel>();
            foreach (var level in _activeLevels)
            {
                if (GetLinkedLevel(level.AssignedProgressiveId, out var linkedLevel))
                {
                    activeLinkedLevels.Add(linkedLevel);
                }
            }

            return activeLinkedLevels;
        }

        public void IncrementProgressiveLevel(string packName, long wager, long ante)
        {
            if (string.IsNullOrEmpty(packName))
            {
                packName = _packName;
            }

            if (_pendingContributions?.Any() ?? false)
            {
                ApplyWagerToActiveLevels(_pendingContributions, 0);
                ApplyMeterIncrements(_pendingContributions, 0);
                _pendingContributions.Clear();
            }
            else
            {
                ApplyWagerToActiveLevels(wager, ante);
                ApplyMeterIncrements(wager, ante);
            }

            _levelProvider.UpdateProgressiveLevels(packName, _gameId, _denomination, _activeLevels);
            TryNotifyUpdate();
        }

        public void SetProgressiveWagerAmounts(IReadOnlyList<long> levelWagers)
        {
            Logger.Debug("IncrementActiveLevels");
            // make sure there is an active game
            if (_activeLevels.Count <= 0)
            {
                return;
            }

            _pendingContributions = levelWagers.Select((w, i) => (Wager: w, Index: i))
                .ToDictionary(x => x.Index, x => x.Wager);
        }

        public void IncrementProgressiveLevelPack(string packName, IEnumerable<ProgressiveLevelUpdate> levelUpdates)
        {
            foreach (var update in levelUpdates)
            {
                var level = GetUniqueLevelIDs().First(l => l.LevelId == update.Id);
                var calculator = _calculatorFactory.Create(level.FundingType);

                calculator?.ApplyContribution(
                    level,
                    update,
                    _meters.GetMeter(level.DeviceId, level.LevelId, ProgressiveMeters.ProgressiveLevelHiddenTotal));

                _meters.GetMeter(level.DeviceId, level.LevelId, ProgressiveMeters.ProgressiveLevelBulkContribution)
                    .Increment(update.Amount);
            }

            _levelProvider.UpdateProgressiveLevels(packName, _gameId, _denomination, _activeLevels);

            TryNotifyUpdate();
        }

        public void UpdateLinkedProgressiveLevels(IEnumerable<IViewableLinkedProgressiveLevel> linkedLevelUpdates)
        {
            lock (_sync)
            {
                foreach (var activeLevel in _activeLevels)
                {
                    if (!GetLinkedLevel(activeLevel.AssignedProgressiveId, out var level))
                    {
                        continue;
                    }

                    activeLevel.CurrentValue = level.Amount.CentsToMillicents();
                }

                if (_activeLevels.Count > 0)
                {
                    _levelProvider.UpdateProgressiveLevels(
                    _packName,
                    _gameId,
                    _denomination,
                    _activeLevels);

                }
            }

            TryNotifyUpdate();
        }

        public IDictionary<int, long> GetProgressiveLevel(string packName, bool isRecovering)
        {
            if (isRecovering && _gameHistory.CurrentLog?.PlayState == PlayState.ProgressivePending
                 && _gameHistory.CurrentLog.Outcomes.Any())
            {
                if (_gameHistory.CurrentLog.Jackpots.Count() == 1)
                {
                    return _gameHistory.CurrentLog.JackpotSnapshot.ToDictionary(x => x.LevelId, x => x.Value);
                }

                var current = GetUniqueLevelIDs().ToDictionary(l => l.LevelId, l => l.CurrentValue);

                var jackpotInfo = _gameHistory.CurrentLog.Jackpots.Last();

                current[jackpotInfo.LevelId] = jackpotInfo.WinAmount;

                return current;
            }

            return GetUniqueLevelIDs().ToDictionary(l => l.LevelId, l => l.CurrentValue);
        }

        public IEnumerable<ProgressiveTriggerResult> TriggerProgressiveLevel(
            string packName,
            IList<int> levelIds,
            IList<long> transactionIds,
            bool recovering)
        {
            var bonusId = _gameStorage.GetValue<string>(
                _gameId,
                _denomination,
                $"{packName}{GamingConstants.BonusKey}");

            lock (_sync)
            {
                using var scope = _storageManager.ScopedTransaction();
                var progressiveTriggers = GetProgressiveTriggerResults(
                    packName,
                    levelIds,
                    transactionIds,
                    recovering,
                    bonusId).ToList();
                _levelProvider.UpdateProgressiveLevels(_packName, _gameId, _denomination, _activeLevels);
                TryNotifyWin();

                scope.Complete();
                return progressiveTriggers;
            }
        }

        public IEnumerable<ClaimResult> ClaimProgressiveLevel(string packName, IList<long> transactionIds)
        {
            var transactions = _transactions.RecallTransactions<JackpotTransaction>();

            foreach (var transactionId in transactionIds)
            {
                var transaction = transactions.FirstOrDefault(t => t.TransactionId == transactionId);
                if (transaction == null)
                {
                    continue;
                }

                yield return new ClaimResult
                {
                    DenomId = transaction.DenomId,
                    GameId = transaction.GameId,
                    ProgressivePackName = packName,
                    LevelId = transaction.LevelId,
                    WinAmount = transaction.WinAmount,
                    WinText = transaction.WinText
                };
            }
        }

        public IEnumerable<PendingProgressivePayout> CommitProgressiveWin(
            IReadOnlyCollection<PendingProgressivePayout> pendingProgressives)
        {
            if (!pendingProgressives.Any())
            {
                return Enumerable.Empty<PendingProgressivePayout>();
            }

            var results = new List<PendingProgressivePayout>();

            lock (_sync)
            {
                foreach (var progressive in pendingProgressives)
                {
                    var transaction = _transactions.RecallTransaction<JackpotTransaction>(progressive.TransactionId);
                    if (transaction == null)
                    {
                        results.Add(progressive);
                        continue;
                    }

                    transaction.PaidAmount = progressive.PaidAmount;
                    transaction.PayMethod = progressive.PayMethod;
                    transaction.PaidDateTime = DateTime.UtcNow;

                    transaction.State = ProgressiveState.Committed;

                    _transactions.UpdateTransaction(transaction);

                    var level = GetUniqueLevelIDs().Single(l => l.LevelId == transaction.LevelId);
                    level.CurrentState = ProgressiveLevelState.Committed;

                    UpdateActiveLevelIfLinked(level);

                    _levelProvider.UpdateProgressiveLevels(_packName, _gameId, _denomination, _activeLevels);

                    _bus.Publish(new ProgressiveCommitEvent(transaction, level));
                }
            }

            return results;
        }

        public void SetProgressiveWin(long transactionId, long winAmount, string winText, PayMethod payMethod)
        {
            lock (_sync)
            {
                var transaction = _transactions.RecallTransaction<JackpotTransaction>(transactionId);
                if (transaction == null)
                {
                    return;
                }

                transaction.State = ProgressiveState.Pending;
                transaction.WinAmount = winAmount;
                transaction.WinText = winText;
                //transaction.WinSequence
                transaction.PayMethod = payMethod;

                var level = GetUniqueLevelIDs().Single(l => l.LevelId == transaction.LevelId);
                using (var scope = _storageManager.ScopedTransaction())
                {
                    _transactions.UpdateTransaction(transaction);

                    _gameHistory.AppendJackpotInfo(
                        new JackpotInfo
                        {
                            DeviceId = transaction.DeviceId,
                            PackName = _packName,
                            HitDateTime = transaction.TransactionDateTime,
                            LevelId = transaction.LevelId,
                            TransactionId = transaction.TransactionId,
                            PayMethod = transaction.PayMethod,
                            WinAmount = transaction.WinAmount,
                            WinText = transaction.WinText,
                            WagerCredits = level.WagerCredits
                        });

                    switch (level.AssignedProgressiveId.AssignedProgressiveType)
                    {
                        case AssignableProgressiveType.Linked:
                            _linkedProgressives.Reset(level.AssignedProgressiveId.AssignedProgressiveKey, transaction);
                            break;
                    }

                    level.CurrentState = ProgressiveLevelState.Pending;
                    _levelProvider.UpdateProgressiveLevels(_packName, _gameId, _denomination, _activeLevels);
                    scope.Complete();
                }

                TryNotifyWin();
            }
        }

        public void IncrementProgressiveWinMeters(IReadOnlyCollection<PendingProgressivePayout> pendingProgressives)
        {
            var transactions = _transactions.RecallTransactions<JackpotTransaction>();
            foreach (var progressive in pendingProgressives)
            {
                var transaction = transactions.FirstOrDefault(t => t.TransactionId == progressive.TransactionId);
                if (transaction == null)
                {
                    continue;
                }

                var level = GetUniqueLevelIDs().Single(l => l.LevelId == transaction.LevelId);
                _meters.GetMeter(level.DeviceId, level.LevelId, ProgressiveMeters.ProgressiveLevelWinOccurrence).Increment(1);
                _meters.GetMeter(level.DeviceId, level.LevelId, ProgressiveMeters.ProgressiveLevelWinAccumulation).Increment(progressive.PaidAmount);
                if (GetSharedSapLevel(level.AssignedProgressiveId, out var sharedLevel))
                {
                    _meters.GetMeter(sharedLevel.Id, ProgressiveMeters.SharedLevelWinOccurrence).Increment(1);
                    _meters.GetMeter(sharedLevel.Id, ProgressiveMeters.SharedLevelWinAccumulation).Increment(progressive.PaidAmount);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private IEnumerable<ProgressiveTriggerResult> GetProgressiveTriggerResults(
            string packName,
            IEnumerable<int> levelIds,
            IList<long> transactionIds,
            bool recovering,
            string bonusId)
        {
            var transactions = _transactions.RecallTransactions<JackpotTransaction>();
            _pendingTransactions.Clear();

            var index = 0;
            foreach (var levelId in levelIds)
            {
                JackpotTransaction transaction;

                var level = GetUniqueLevelIDs().First(l => l.LevelId == levelId);

                // If there's no corresponding transaction Id a new transaction must be created
                //  There may be an associated transaction during recovery
                var transactionId = transactionIds.ElementAtOrDefault(index++);
                if (transactionId == 0)
                {
                    transaction = new JackpotTransaction(
                        level.DeviceId,
                        DateTime.UtcNow,
                        level.ProgressiveId,
                        level.LevelId,
                        _gameId,
                        _denomination,
                        1, // TODO: get win level index from game
                        level.CurrentValue,
                        string.Empty,
                        1L,
                        level.ResetValue,
                        (int)level.AssignedProgressiveId.AssignedProgressiveType,
                        level.AssignedProgressiveId?.AssignedProgressiveKey ?? string.Empty,
                        PayMethod.Any,
                        level.HiddenValue,
                        level.Overflow)
                    { BonusId = bonusId };

                    level.CurrentState = ProgressiveLevelState.Hit;
                    _transactions.AddTransaction(transaction);
                }
                else
                {
                    transaction = transactions.First(t => t.TransactionId == transactionId);
                    Logger.Debug($"Retrieved transaction {transaction}");
                }

                _pendingTransactions.Add(transaction.TransactionId);

                if (transaction.State == ProgressiveState.Hit)
                {
                    // This notifies the various providers (like the SAP provider) that there's a hit to handle
                    _bus.Publish(new ProgressiveHitEvent(transaction, level, recovering));
                }

                yield return new ProgressiveTriggerResult
                {
                    GameId = _gameId,
                    Denom = _denomination,
                    LevelId = levelId,
                    ProgressivePackName = packName,
                    TransactionId = transaction.TransactionId
                };
            }
        }

        private void ApplyMeterIncrements(long wager, long ante)
        {
            ApplyMeterIncrements(
                GetUniqueLevelIDs().ToDictionary(x => x.LevelId, _ => wager),
                ante);
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This will be used when G2S progressive support is added")]
        private void ApplyMeterIncrements(IDictionary<int, long> wagers, long ante)
        {
            foreach (var level in GetUniqueLevelIDs())
            {
                if (!wagers.TryGetValue(level.LevelId, out var wager))
                {
                    throw new InvalidOperationException($"Missing a wager for levelId {level.LevelId}");
                }

                _meters.GetMeter(level.DeviceId, level.LevelId, ProgressiveMeters.ProgressiveLevelWageredAmount)
                    .Increment(wager);
            }
        }

        private void ApplyWagerToActiveLevels(long wager, long ante)
        {
            ApplyWagerToActiveLevels(
                GetUniqueLevelIDs().ToDictionary(x => x.LevelId, _ => wager),
                ante);
        }

        private void ApplyWagerToActiveLevels(IDictionary<int, long> wagers, long ante)
        {
            foreach (var level in GetUniqueLevelIDs().OrderBy(x => x.LevelId))
            {
                var levelType = level.LevelType;
                var assignedIdType = level.AssignedProgressiveId.AssignedProgressiveType;
                if (!wagers.TryGetValue(level.LevelId, out var wager))
                {
                    throw new InvalidOperationException($"Missing a wager for levelId {level.LevelId}");
                }

                switch (levelType)
                {
                    case ProgressiveLevelType.Sap when assignedIdType == AssignableProgressiveType.AssociativeSap:
                    case ProgressiveLevelType.Selectable when assignedIdType == AssignableProgressiveType.CustomSap:
                        {
                            _sharedSap.Increment(level, wager, ante);
                            break;
                        }
                    case ProgressiveLevelType.Sap:
                        {
                            _standaloneProgressives.Increment(
                                level,
                                wager,
                                ante,
                                _meters.GetMeter(
                                    level.DeviceId,
                                    level.LevelId,
                                    ProgressiveMeters.ProgressiveLevelHiddenTotal));
                            break;
                        }
                    case ProgressiveLevelType.LP:
                        {
                            // do nothing, increments come from server
                            break;
                        }
                    case ProgressiveLevelType.Selectable when assignedIdType == AssignableProgressiveType.Linked:
                        {
                            // do nothing, increments come from server
                            break;
                        }
                    default:
                        {
                            throw new InvalidOperationException("Attempt to increment on unknown progressive type");
                        }
                }
            }
        }

        private void Reset()
        {
            lock (_sync)
            {
                _gameId = 1;
                _denomination = 0;
                _packName = string.Empty;
                _pendingContributions.Clear();

                foreach (var level in _activeLevels)
                {
                    level.CurrentState = ProgressiveLevelState.Ready;
                }

                _activeLevels = new List<ProgressiveLevel>();
                _pendingTransactions.Clear();
            }
        }

        private IEnumerable<ProgressiveLevel> GetUniqueLevelIDs()
        {
            var progressivePoolCreationType = _propertiesManager.GetValue(
                GamingConstants.ProgressivePoolCreationType,
                ProgressivePoolCreation.Default);

            var creationType = _activeLevels.FirstOrDefault()?.CreationType ?? LevelCreationType.Default;

            if (progressivePoolCreationType == ProgressivePoolCreation.Default ||
                creationType == LevelCreationType.Default)
            {
                return _activeLevels;
            }

            var betCreditsSaved = _propertiesManager.GetValue(GamingConstants.SelectedBetCredits, (long)0);
            return _activeLevels.Where(x => x.WagerCredits == betCreditsSaved);

        }

        private void TryNotifyUpdate()
        {
            if (_runtime.Connected)
            {
                _runtime.JackpotNotification();
            }

            Logger.Debug($"Jackpot notification invoked for {_packName}");
        }

        private void TryNotifyWin()
        {
            var pending = _transactions.RecallTransactions<JackpotTransaction>()
                .Where(t => _pendingTransactions.Contains(t.TransactionId)).ToList();

            if (pending.All(l => l.State != ProgressiveState.Hit))
            {
                var wins = pending.ToDictionary(t => t.LevelId, t => t.TransactionId);

                _runtime.JackpotWinNotification(_packName, wins);

                Logger.Debug($"Jackpot win notification invoked for {_packName}");
            }
        }

        private bool GetLinkedLevel(AssignableProgressiveId progressiveId, out IViewableLinkedProgressiveLevel outLevel)
        {
            if (progressiveId.AssignedProgressiveType != AssignableProgressiveType.Linked)
            {
                outLevel = null;
                return false;
            }

            if (!_linkedProgressives.ViewLinkedProgressiveLevel(progressiveId.AssignedProgressiveKey, out var level))
            {
                // If we have an assigned progressive id and it is linked, but we can't find the key, we have
                // a big problem. The level may have become unassigned at some point.
                throw new InvalidOperationException(
                    $"Unable to find assigned linked progressive: {progressiveId.AssignedProgressiveKey}");
            }

            outLevel = level;
            return true;
        }

        private bool GetSharedSapLevel(AssignableProgressiveId progressiveId, out IViewableSharedSapLevel outLevel)
        {
            if (progressiveId.AssignedProgressiveType == AssignableProgressiveType.CustomSap
                || progressiveId.AssignedProgressiveType == AssignableProgressiveType.AssociativeSap)
            {
                if (!_sharedSap.ViewSharedSapLevel(progressiveId.AssignedProgressiveKey, out outLevel))
                {
                    // If we have an assigned progressive id and it is shared sap, but we can't find the key, we have
                    // a big problem. The level may have become unassigned at some point.
                    throw new InvalidOperationException(
                        $"Unable to find assigned shared sap progressive: {progressiveId.AssignedProgressiveKey}");
                }

                return true;
            }

            outLevel = null;
            return false;
        }

        private void UpdateActiveLevelIfLinked(ProgressiveLevel level)
        {
            if (GetLinkedLevel(level.AssignedProgressiveId, out var linkedLevel))
            {
                level.CurrentValue = linkedLevel.Amount.CentsToMillicents();
            }
        }

        private void Handle(ProgressiveAwardedEvent theEvent)
        {
            SetProgressiveWin(theEvent.TransactionId, theEvent.AwardedAmount, theEvent.WinText, theEvent.PayMethod);
        }
    }
}