﻿namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Contracts.Meters;
    using Contracts.Progressives;
    using Contracts.Progressives.SharedSap;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <inheritdoc cref="ISharedSapProvider" />
    public sealed class SharedSapProvider : ISharedSapProvider, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly ConcurrentDictionary<Guid, SharedSapLevel> _sharedSapIndex;
        private readonly IEventBus _eventBus;
        private readonly IPersistentBlock _saveBlock;
        private readonly IProgressiveCalculatorFactory _calculatorFactory;
        private readonly IProgressiveMeterManager _meters;
        private readonly IMysteryProgressiveProvider _mysteryProgressiveProvider;
        private readonly string _saveKey;

        private readonly SharedSapIndexStorage _indexStorage;

        public SharedSapProvider(
            IEventBus eventBus,
            IPersistenceProvider persistenceProvider,
            IPropertiesManager propertiesManager,
            IProgressiveCalculatorFactory calculatorFactory,
            IProgressiveMeterManager meters,
            IMysteryProgressiveProvider mysteryProgressiveProvider)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _mysteryProgressiveProvider = mysteryProgressiveProvider ?? throw new ArgumentNullException(nameof(mysteryProgressiveProvider));
            _calculatorFactory = calculatorFactory ?? throw new ArgumentNullException(nameof(calculatorFactory));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));

            _saveKey = nameof(SharedSapProvider);

            var persistenceLevel = propertiesManager.GetValue(ApplicationConstants.DemonstrationMode, false)
                ? PersistenceLevel.Transient
                : PersistenceLevel.Static;

            _saveBlock = persistenceProvider?.GetOrCreateBlock(_saveKey, persistenceLevel) ??
                         throw new ArgumentNullException(nameof(persistenceProvider));

            _indexStorage = _saveBlock.GetOrCreateValue<SharedSapIndexStorage>(_saveKey);

            _sharedSapIndex = _indexStorage?.SharedSapIndex ??
                              throw new ArgumentNullException(nameof(_indexStorage));

            _eventBus.Subscribe<ProgressiveHitEvent>(this, Handle);
        }

        public void Dispose()
        {
            _saveBlock?.Dispose();
            _eventBus?.UnsubscribeAll(this);
        }

        public string Name => nameof(SharedSapProvider);

        public ICollection<Type> ServiceTypes => new[] { typeof(ISharedSapProvider) };

        public void Initialize()
        {
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public IEnumerable<IViewableSharedSapLevel> AddSharedSapLevel(
            IEnumerable<IViewableSharedSapLevel> sharedSapLevels)
        {
            CheckForNull(sharedSapLevels);

            var addedLevels = new List<IViewableSharedSapLevel>();

            foreach (var sharedSapLevel in sharedSapLevels)
            {
                if (_sharedSapIndex.ContainsKey(sharedSapLevel.Id))
                {
                    continue;
                }

                var levelToAdd = new SharedSapLevel
                {
                    Id = sharedSapLevel.Id, // we do this once on add and it never changes
                    Name = sharedSapLevel.Name,
                    LevelId = sharedSapLevel.LevelId, // should be unique per game type
                    SupportedGameTypes = sharedSapLevel.SupportedGameTypes.ToArray(),
                    IncrementRate = sharedSapLevel.IncrementRate,
                    InitialValue = sharedSapLevel.InitialValue,
                    ResetValue = sharedSapLevel.ResetValue,
                    MaximumValue = sharedSapLevel.MaximumValue,
                    CurrentValue = sharedSapLevel.CurrentValue,
                    HiddenIncrementRate = sharedSapLevel.HiddenIncrementRate,
                    CurrentErrorStatus = ProgressiveErrors.None,
                    CanEdit = sharedSapLevel.CanEdit,
                    AutoGenerated = sharedSapLevel.AutoGenerated,
                    CreatedDateTime = DateTime.UtcNow,
                };

                CheckLevelForErrors(levelToAdd);

                if (_sharedSapIndex.TryAdd(levelToAdd.Id, levelToAdd))
                {
                    addedLevels.Add(levelToAdd);
                    Logger.Debug($"Shared Level Added: {levelToAdd}");
                }
            }

            _meters.AddProgressives(addedLevels);

            Save();

            _eventBus.Publish(new SharedSapAddedEvent(addedLevels));

            return addedLevels;
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public IEnumerable<IViewableSharedSapLevel> RemoveSharedSapLevel(
            IEnumerable<IViewableSharedSapLevel> sharedSapLevels)
        {
            CheckForNull(sharedSapLevels);

            var removedLevels = new List<SharedSapLevel>();

            foreach (var sharedSapLevel in sharedSapLevels)
            {
                if (!sharedSapLevel.CanEdit)
                {
                    continue;
                }

                if (_sharedSapIndex.TryRemove(sharedSapLevel.Id, out var removedLevel))
                {
                    removedLevels.Add(removedLevel);
                    Logger.Debug($"Shared Level Removed: {removedLevel}");
                }
            }

            Save();

            // TODO: If there are any level assignments, they need to be removed
            _eventBus.Publish(new SharedSapRemovedEvent(removedLevels));

            return removedLevels;
        }

        public bool ViewSharedSapLevel(string assignmentKey, out IViewableSharedSapLevel level)
        {
            level = _sharedSapIndex.Values.SingleOrDefault(x => x.LevelAssignmentKey == assignmentKey);
            return level != null;
        }

        public IEnumerable<IViewableSharedSapLevel> Save()
        {
            using (var transaction = _saveBlock.Transaction())
            {
                transaction.SetValue(_saveKey, _indexStorage);
                transaction.Commit();
            }

            _eventBus.Publish(new SharedSapSavedEvent(_sharedSapIndex.Values));

            return _sharedSapIndex.Values;
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public IEnumerable<IViewableSharedSapLevel> UpdateSharedSapLevel(
            IEnumerable<IViewableSharedSapLevel> levelUpdates)
        {
            CheckForNull(levelUpdates);

            var updatedLevels = new List<SharedSapLevel>();

            foreach (var sharedSapLevel in levelUpdates)
            {
                if (!_sharedSapIndex.TryGetValue(sharedSapLevel.Id, out var levelToUpdate))
                {
                    continue;
                }

                if (levelToUpdate.CanEdit)
                {
                    // If we can edit, then we haven't contributed to the level yet
                    // so we can update everything except the GUID and levelId
                    levelToUpdate.Name = sharedSapLevel.Name;
                    levelToUpdate.LevelId = sharedSapLevel.LevelId;
                    levelToUpdate.SupportedGameTypes = sharedSapLevel.SupportedGameTypes.ToArray();
                    levelToUpdate.InitialValue = sharedSapLevel.InitialValue;
                    levelToUpdate.ResetValue = sharedSapLevel.ResetValue;
                    levelToUpdate.IncrementRate = sharedSapLevel.IncrementRate;
                    levelToUpdate.HiddenIncrementRate = sharedSapLevel.HiddenIncrementRate;
                    levelToUpdate.MaximumValue = sharedSapLevel.MaximumValue;
                    levelToUpdate.CanEdit = sharedSapLevel.CanEdit;
                }

                levelToUpdate.CurrentValue = sharedSapLevel.CurrentValue;
                CheckLevelForErrors(levelToUpdate);
                updatedLevels.Add(levelToUpdate);
                Logger.Debug($"Shared Level Updated: {levelToUpdate}");
            }

            Save();

            _eventBus.Publish(new SharedSapUpdatedEvent(updatedLevels));

            return updatedLevels;
        }

        public void Increment(ProgressiveLevel level, long wager, long ante)
        {
            var assignmentIdType = level.AssignedProgressiveId.AssignedProgressiveType;
            var levelKey = level.AssignedProgressiveId.AssignedProgressiveKey;

            switch (level.LevelType)
            {
                case ProgressiveLevelType.Sap when assignmentIdType == AssignableProgressiveType.AssociativeSap:
                case ProgressiveLevelType.Selectable when assignmentIdType == AssignableProgressiveType.CustomSap:
                    {
                        var calculator = _calculatorFactory.Create(SapFundingType.Standard);
                        var sharedSapLevel = GetLevelByKey(levelKey);

                        level.IncrementRate = sharedSapLevel.IncrementRate;
                        level.HiddenIncrementRate = sharedSapLevel.HiddenIncrementRate;
                        level.HiddenValue = sharedSapLevel.HiddenValue;
                        level.Overflow = sharedSapLevel.Overflow;
                        level.OverflowTotal = sharedSapLevel.OverflowTotal;
                        level.MaximumValue = sharedSapLevel.MaximumValue;

                        var hiddenTotalMeter = _meters.GetMeter(
                            level.DeviceId,
                            level.LevelId,
                            ProgressiveMeters.ProgressiveLevelHiddenTotal);

                        var hiddenValue = hiddenTotalMeter.Lifetime;

                        calculator?.Increment(
                            level,
                            wager,
                            ante,
                            hiddenTotalMeter);
                        
                        sharedSapLevel.CanEdit = false; // Once we update we can no longer edit the level ever again

                        UpdateSharedSapLevel(level, sharedSapLevel);
                        sharedSapLevel.HiddenTotal += hiddenTotalMeter.Lifetime - hiddenValue;

                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException("Invalid increment on unsupported level type");
                    }
            }

            Save();
        }
        
        public void ProcessHit(ProgressiveLevel level, IViewableJackpotTransaction transaction)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            var sharedSapLevel = GetLevelByKey(level.AssignedProgressiveId.AssignedProgressiveKey);
            var calculator = _calculatorFactory.Create(SapFundingType.Standard);
            var award = calculator.Claim(level, sharedSapLevel.ResetValue);
            UpdateSharedSapLevel(level, sharedSapLevel);

            if (level.TriggerControl == TriggerType.Mystery)
            {
                _mysteryProgressiveProvider.GenerateMagicNumber(level);
            }

            Save();

            _eventBus.Publish(new SharedSapAwardedEvent(transaction.TransactionId, award, string.Empty, PayMethod.Any));
        }

        public void Reset(ProgressiveLevel level)
        {
            var sharedSapLevel = GetLevelByKey(level.AssignedProgressiveId.AssignedProgressiveKey);
            var calculator = _calculatorFactory.Create(SapFundingType.Standard);
            calculator.Reset(level, sharedSapLevel.ResetValue);

            UpdateSharedSapLevel(level, sharedSapLevel);

            Save();
        }

        public IEnumerable<IViewableSharedSapLevel> ViewSharedSapLevels()
        {
            return _sharedSapIndex.Values.AsEnumerable();
        }


        private static void UpdateSharedSapLevel(IViewableProgressiveLevel level, SharedSapLevel sharedSapLevel)
        {
            sharedSapLevel.CurrentValue = level.CurrentValue;
            sharedSapLevel.Residual = level.Residual;
            sharedSapLevel.Overflow = level.Overflow;
            sharedSapLevel.OverflowTotal = level.OverflowTotal;
            sharedSapLevel.HiddenIncrementRate = level.HiddenIncrementRate;
            sharedSapLevel.HiddenValue = level.HiddenValue;
        }

        private static void CheckForNull(IEnumerable<IViewableSharedSapLevel> levels)
        {
            if (levels == null)
            {
                throw new ArgumentNullException(nameof(levels));
            }
        }

        private static void CheckLevelForErrors(SharedSapLevel levelToAdd)
        {
            if (!levelToAdd.CurrentErrorStatus.HasFlag(ProgressiveErrors.MinimumThresholdNotReached) &&
                (levelToAdd.InitialValue < levelToAdd.ResetValue ||
                 levelToAdd.CurrentValue < levelToAdd.ResetValue))
            {
                levelToAdd.CurrentErrorStatus |= ProgressiveErrors.MinimumThresholdNotReached;
                Logger.Debug($"Minimum Threshold Error Detected: {levelToAdd}");
            }
            else if (levelToAdd.CurrentErrorStatus.HasFlag(ProgressiveErrors.MinimumThresholdNotReached) &&
                     levelToAdd.InitialValue >= levelToAdd.ResetValue &&
                     levelToAdd.CurrentValue >= levelToAdd.ResetValue)
            {
                levelToAdd.CurrentErrorStatus &= ~ProgressiveErrors.MinimumThresholdNotReached;
                Logger.Debug($"Minimum Threshold Error Removed: {levelToAdd}");
            }
        }

        private SharedSapLevel GetLevelByKey(string assignmentKey)
        {
            return _sharedSapIndex.Values.Single(x => x.LevelAssignmentKey == assignmentKey);
        }

        private void Handle(ProgressiveHitEvent theEvent)
        {
            var assignedLevelId = theEvent.Level.AssignedProgressiveId;

            if ((theEvent.Level.LevelType == ProgressiveLevelType.Selectable ||
                 theEvent.Level.LevelType == ProgressiveLevelType.Sap) &&
                (assignedLevelId.AssignedProgressiveType == AssignableProgressiveType.CustomSap ||
                 assignedLevelId.AssignedProgressiveType == AssignableProgressiveType.AssociativeSap))
            {
                ProcessHit(theEvent.Level as ProgressiveLevel, theEvent.Jackpot);
            }
        }

        internal class SharedSapIndexStorage
        {
            public ConcurrentDictionary<Guid, SharedSapLevel> SharedSapIndex { get; set; } =
                new ConcurrentDictionary<Guid, SharedSapLevel>();
        }
    }
}