namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Quartz.Util;

    public class ProgressiveConfigurationProvider : IProgressiveConfigurationProvider, IService, IDisposable
    {
        private readonly IProgressiveLevelProvider _levelProvider;
        private readonly ILinkedProgressiveProvider _linkedProgressiveProvider;
        private readonly ISharedSapProvider _sharedSapProvider;
        private readonly IPersistentStorageManager _storage;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;

        private readonly ConcurrentDictionary<(int gameId, long denom), IList<ProgressiveLevel>> _levelCache =
            new ConcurrentDictionary<(int gameId, long denom), IList<ProgressiveLevel>>();
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly double _multiplier;

        private bool _disposed;

        public ProgressiveConfigurationProvider(
            IProgressiveLevelProvider levelProvider,
            ILinkedProgressiveProvider linkedLevelProvider,
            ISharedSapProvider sharedSapProvider,
            IPropertiesManager properties,
            IPersistentStorageManager storage,
            IEventBus eventBus)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _levelProvider = levelProvider ?? throw new ArgumentNullException(nameof(levelProvider));
            _linkedProgressiveProvider =
                linkedLevelProvider ?? throw new ArgumentNullException(nameof(linkedLevelProvider));
            _sharedSapProvider = sharedSapProvider ?? throw new ArgumentNullException(nameof(sharedSapProvider));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _multiplier = properties.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
            AddLevelCacheData(_levelProvider.GetProgressiveLevels());
            _eventBus.Subscribe<GameAddedEvent>(this, Handle);

            _levelProvider.ProgressivesLoaded += UpdateLevelCache;
        }

        public IReadOnlyCollection<IViewableProgressiveLevel> AssignLevelsToGame(
            IReadOnlyCollection<ProgressiveLevelAssignment> levelAssignments)
        {
            if (levelAssignments == null)
            {
                throw new ArgumentNullException(nameof(levelAssignments));
            }

            var levelAssignmentUpdates = new List<ProgressiveLevel>();

            if (levelAssignments.Count == 0)
            {
                // Right now the updates are all or nothing, so if the count is
                // zero we abort immediately
                return levelAssignmentUpdates;
            }

            var levelGroups = levelAssignments.GroupBy(
                x => (game: x.GameDetail, denom: x.Denom, packName: x.ProgressiveLevel.ProgressivePackName));
            var updatedLevels = new List<(string packName, int gameId, long denom, IEnumerable<ProgressiveLevel> levels)>();
            var sharedSapLevels = new List<IViewableSharedSapLevel>();
            foreach (var levelGroup in levelGroups)
            {
                var game = levelGroup.Key.game;
                var denom = levelGroup.Key.denom;
                var packName = levelGroup.Key.packName;
                var betOption = levelGroup
                    .FirstOrDefault(l => !l.ProgressiveLevel.BetOption.IsNullOrWhiteSpace())
                    ?.ProgressiveLevel.BetOption;

                var associatedLevels = GetProgressiveLevels(game.Id, denom, packName)
                    .Where(l => l.BetOption.IsNullOrEmpty() || l.BetOption == betOption).ToList();

                Logger.Debug($" AssociatedLevels count = {associatedLevels.Count}");

                var progressiveLevels = ApplyLevelMapping(levelGroup, associatedLevels).ToList();
                levelAssignmentUpdates.AddRange(progressiveLevels);
                ValidateProgressiveConfiguration(associatedLevels);

                updatedLevels.Add((packName, game.Id, denom, progressiveLevels));

                foreach (var level in progressiveLevels)
                {
                    if (level.AssignedProgressiveId.AssignedProgressiveType ==
                        AssignableProgressiveType.AssociativeSap)
                    {
                        if (_sharedSapProvider.ViewSharedSapLevel(
                            level.AssignedProgressiveId.AssignedProgressiveKey,
                            out var sharedLevel))
                        {
                            if (sharedLevel is SharedSapLevel update && sharedLevel.CanEdit)
                            {
                                update.CurrentValue = update.InitialValue = level.InitialValue;

                                sharedSapLevels.Add(sharedLevel);
                            }
                        }

                        var updateAssociatedLevels = _levelProvider.GetProgressiveLevels()
                            .Select(l => l).Where(
                                l => l.AssignedProgressiveId.AssignedProgressiveKey ==
                                     level.AssignedProgressiveId.AssignedProgressiveKey &&
                                     l.LevelName.Equals(level.LevelName) &&
                                     l.GameId != level.GameId).ToList();

                        foreach (var associatedLevel in updateAssociatedLevels)
                        {
                            associatedLevel.CurrentValue = associatedLevel.InitialValue = level.InitialValue;

                            var updatePackLevels = updatedLevels.FirstOrDefault(
                                u => u.packName == associatedLevel.ProgressivePackName &&
                                     u.gameId == associatedLevel.GameId &&
                                     associatedLevel.Denomination.Contains(u.denom));

                            if (updatePackLevels.Equals(default((string, int, long, IList<ProgressiveLevel>))))
                            {
                                updatedLevels.Add(
                                    (associatedLevel.ProgressivePackName, associatedLevel.GameId,
                                        associatedLevel.Denomination.First(),
                                        new List<ProgressiveLevel> { associatedLevel }));
                            }
                            else
                            {
                                (updatePackLevels.levels as List<ProgressiveLevel>)?.Add(associatedLevel);
                            }
                        }
                    }
                }

                _eventBus.Publish(new GameDenomChangedEvent(game.Id, game, denom, _multiplier));
            }
            
            using var storage = _storage.ScopedTransaction();
            if (sharedSapLevels.Any())
            {
                _sharedSapProvider.UpdateSharedSapLevel(sharedSapLevels);
            }

            _levelProvider.UpdateProgressiveLevels(updatedLevels);
            storage.Complete();

            return levelAssignmentUpdates;
        }

        public void LockProgressiveLevels(IReadOnlyCollection<IViewableProgressiveLevel> progressiveLevels)
        {
            var progressiveGrouping = progressiveLevels.SelectMany(
                    x => x.Denomination.Select(d => (pack: x.ProgressivePackName, gameId: x.GameId, denom: d)))
                .Distinct()
                .Select(
                    x => (pack: x.pack, gameId: x.gameId, denom: x.denom,
                        levels: GetProgressiveLevels(x.gameId, x.denom, x.pack).Select(
                            level =>
                            {
                                if (level.CanEdit && progressiveLevels.Any(
                                    l => l.LevelId == level.LevelId && level.ProgressivePackName == l.ProgressivePackName))
                                {
                                    level.CanEdit = false;
                                    level.CurrentState = ProgressiveLevelState.Ready;
                                }

                                return level;
                            })));
            using (var scope = _storage.ScopedTransaction())
            {
                _levelProvider.UpdateProgressiveLevels(progressiveGrouping);
                scope.Complete();
            }
        }

        public ProgressiveErrors ValidateLinkedProgressive(
            IViewableLinkedProgressiveLevel linkedProgressiveLevel,
            IViewableProgressiveLevel progressiveLevel)
        {
            if (progressiveLevel.AssignedProgressiveId.AssignedProgressiveType != AssignableProgressiveType.Linked ||
                progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey != linkedProgressiveLevel.LevelName)
            {
                throw new ArgumentException(
                    "The provided progressive level is not valid for this linked progressive level");
            }

            if (progressiveLevel.ResetValue.MillicentsToCents() <= linkedProgressiveLevel.Amount)
            {
                return linkedProgressiveLevel.CurrentErrorStatus;
            }

            return linkedProgressiveLevel.CurrentErrorStatus | ProgressiveErrors.MinimumThresholdNotReached;
        }

        public void ValidateLinkedProgressivesUpdates(IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            var clearedErrors = new List<IViewableProgressiveLevel>();
            var addedErrors = new List<IViewableProgressiveLevel>();
            var progressiveLevels = levels.ToDictionary(x => x.LevelName);
            var assignedLinkLevels = _levelProvider.GetProgressiveLevels().Where(
                level => level.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                         progressiveLevels.Keys.Contains(level.AssignedProgressiveId.AssignedProgressiveKey));
            foreach (var level in assignedLinkLevels)
            {
                var linkedProgressiveLevel = progressiveLevels[level.AssignedProgressiveId.AssignedProgressiveKey];
                ValidateLinkedProgressiveLevel(level, linkedProgressiveLevel, clearedErrors, addedErrors);
            }

            PostEventIfAny(
                clearedErrors,
                () => _eventBus.Publish(new ProgressiveMinimumThresholdClearedEvent(clearedErrors)));
            PostEventIfAny(
                addedErrors,
                () => _eventBus.Publish(new ProgressiveMinimumThresholdErrorEvent(addedErrors)));
        }

        public IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels()
        {
            return _levelProvider.GetProgressiveLevels();
        }

        public IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels(
            int gameId,
            long denom,
            string progressivePackName)
        {
            return GetProgressiveLevels(gameId, denom, progressivePackName);
        }

        public IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels(int gameId, long denom)
        {
            return GetProgressiveLevels(gameId, denom);
        }

        public IEnumerable<IViewableProgressiveLevel> ViewConfiguredProgressiveLevels()
        {
            return _levelProvider.GetProgressiveLevels()
                .Where(level => level.CurrentState != ProgressiveLevelState.Init);
        }

        public IEnumerable<IViewableProgressiveLevel> ViewConfiguredProgressiveLevels(int gameId, long denom)
        {
            return GetProgressiveLevels(gameId, denom).Where(x => x.CurrentState != ProgressiveLevelState.Init);
        }

        public IReadOnlyCollection<IViewableSharedSapLevel> ViewSharedSapLevels()
        {
            return _sharedSapProvider.ViewSharedSapLevels().ToList();
        }

        public IReadOnlyCollection<IViewableLinkedProgressiveLevel> ViewLinkedProgressiveLevels()
        {
            return _linkedProgressiveProvider.ViewLinkedProgressiveLevels().ToList();
        }

        public string Name => nameof(ProgressiveConfigurationProvider);

        public ICollection<Type> ServiceTypes => new[] { typeof(IProgressiveConfigurationProvider) };

        public void Initialize()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private IEnumerable<ProgressiveLevel> GetProgressiveLevels(int gameId, long denom)
        {
            return _levelCache.TryGetValue((gameId, denom), out var levels)
                ? levels
                : Enumerable.Empty<ProgressiveLevel>();
        }

        private IEnumerable<ProgressiveLevel> GetProgressiveLevels(int gameId, long denom, string packName)
        {
            return GetProgressiveLevels(gameId, denom).Where(x => x.ProgressivePackName == packName);
        }

        private void Handle(GameAddedEvent evt)
        {
            AddLevelCacheData(_levelProvider.GetProgressiveLevels().Where(x => x.GameId == evt.GameId));
        }

        private void AddLevelCacheData(IEnumerable<ProgressiveLevel> progressiveLevels)
        {
            var levelGrouping = progressiveLevels.SelectMany(x => x.Denomination.Select(d => (denom: d, level: x))).GroupBy(
                x => (x.level.GameId, x.denom),
                x => x.level);
            foreach (var levelGroup in levelGrouping)
            {
                _levelCache.TryAdd(levelGroup.Key, levelGroup.ToList());
            }
        }

        private IEnumerable<ProgressiveLevel> ApplyLevelMapping(
            IEnumerable<ProgressiveLevelAssignment> levelAssignments,
            IReadOnlyCollection<ProgressiveLevel> associatedLevels)
        {
            var progressivePoolCreationType = _properties.GetValue(
                GamingConstants.ProgressivePoolCreationType,
                ProgressivePoolCreation.Default);

            foreach (var assignment in levelAssignments)
            {
                ProgressiveLevel progressiveLevel;

                // There should be exactly one level that matches
                if (progressivePoolCreationType != ProgressivePoolCreation.WagerBased &&
                    assignment.ProgressiveLevel.CreationType != LevelCreationType.Default)
                {
                    progressiveLevel = associatedLevels
                        .Single(
                            x => x.LevelId == assignment.ProgressiveLevel.LevelId
                                 && x.LevelName == assignment.ProgressiveLevel.LevelName);
                }
                else
                {
                    progressiveLevel = associatedLevels
                        .Single(
                            x => x.LevelId == assignment.ProgressiveLevel.LevelId
                                 && x.LevelName == assignment.ProgressiveLevel.LevelName
                                 && x.WagerCredits == assignment.ProgressiveLevel.WagerCredits);
                }

                progressiveLevel.AssignedProgressiveId = assignment.AssignedProgressiveIdInfo;
                progressiveLevel.CurrentState = ProgressiveLevelState.Ready;

                if (progressiveLevel.LevelType == ProgressiveLevelType.Sap && progressiveLevel.CanEdit)
                {
                    progressiveLevel.CurrentValue = progressiveLevel.InitialValue = assignment.InitialValue;
                }

                yield return progressiveLevel;
            }
        }

        private static void ValidateSharedSapProgressLevel(
            ProgressiveLevel level,
            IViewableSharedSapLevel sharedSapLevel,
            List<IViewableProgressiveLevel> clearedErrors,
            List<IViewableProgressiveLevel> addedErrors)
        {
            if (level.Errors.HasFlag(ProgressiveErrors.MinimumThresholdNotReached) &&
                level.ResetValue <= sharedSapLevel.ResetValue)
            {
                level.Errors &= ~ProgressiveErrors.MinimumThresholdNotReached;
                clearedErrors.Add(level);
            }
            else if (!level.Errors.HasFlag(ProgressiveErrors.MinimumThresholdNotReached) &&
                     level.ResetValue > sharedSapLevel.ResetValue)
            {
                level.Errors |= ProgressiveErrors.MinimumThresholdNotReached;
                addedErrors.Add(level);
            }
        }

        private static void ValidateLinkedProgressiveLevel(
            ProgressiveLevel level,
            IViewableLinkedProgressiveLevel linkedProgressiveLevel,
            List<IViewableProgressiveLevel> clearedErrors,
            List<IViewableProgressiveLevel> addedErrors)
        {
            if (level.Errors.HasFlag(ProgressiveErrors.MinimumThresholdNotReached) &&
                level.ResetValue.MillicentsToCents() <= linkedProgressiveLevel.Amount)
            {
                level.Errors &= ~ProgressiveErrors.MinimumThresholdNotReached;
                clearedErrors.Add(level);
            }
            else if (!level.Errors.HasFlag(ProgressiveErrors.MinimumThresholdNotReached) &&
                     level.ResetValue.MillicentsToCents() > linkedProgressiveLevel.Amount)
            {
                level.Errors |= ProgressiveErrors.MinimumThresholdNotReached;
                addedErrors.Add(level);
            }
        }

        private static void PostEventIfAny(IEnumerable<IViewableProgressiveLevel> levels, Action postEvent)
        {
            if (levels.Any())
            {
                postEvent();
            }
        }

        private void UpdateLevelCache(object sender, ProgressivesLoadedEventArgs eventArgs)
        {
            AddLevelCacheData(eventArgs.ProgressiveLevels);
        }

        private void ValidateProgressiveConfiguration(IEnumerable<ProgressiveLevel> levels)
        {
            var clearedErrors = new List<IViewableProgressiveLevel>();
            var addedErrors = new List<IViewableProgressiveLevel>();
            foreach (var level in levels)
            {
                ValidateProgressiveLevel(level, clearedErrors, addedErrors);
            }

            PostEventIfAny(
                clearedErrors,
                () => _eventBus.Publish(new ProgressiveMinimumThresholdClearedEvent(clearedErrors)));
            PostEventIfAny(
                addedErrors,
                () => _eventBus.Publish(new ProgressiveMinimumThresholdErrorEvent(addedErrors)));
        }

        private void ValidateProgressiveLevel(
            ProgressiveLevel level,
            List<IViewableProgressiveLevel> clearedErrors,
            List<IViewableProgressiveLevel> addedErrors)
        {
            switch (level.AssignedProgressiveId.AssignedProgressiveType)
            {
                case AssignableProgressiveType.None:
                    if (level.Errors.HasFlag(ProgressiveErrors.MinimumThresholdNotReached) &&
                        level.ResetValue <= level.CurrentValue)
                    {
                        level.Errors &= ~ProgressiveErrors.MinimumThresholdNotReached;
                        clearedErrors.Add(level);
                    }
                    else if (!level.Errors.HasFlag(ProgressiveErrors.MinimumThresholdNotReached) &&
                             level.ResetValue > level.CurrentValue)
                    {
                        level.Errors |= ProgressiveErrors.MinimumThresholdNotReached;
                        addedErrors.Add(level);
                    }

                    break;
                case AssignableProgressiveType.CustomSap:
                case AssignableProgressiveType.AssociativeSap:
                    if (!_sharedSapProvider.ViewSharedSapLevel(
                        level.AssignedProgressiveId.AssignedProgressiveKey,
                        out var sharedSapLevel))
                    {
                        throw new ArgumentException("Invalid shared SAP configuration progressive configuration");
                    }

                    ValidateSharedSapProgressLevel(level, sharedSapLevel, clearedErrors, addedErrors);
                    break;
                case AssignableProgressiveType.Linked:
                    if (!_linkedProgressiveProvider.ViewLinkedProgressiveLevel(
                        level.AssignedProgressiveId.AssignedProgressiveKey,
                        out var linkedLevel))
                    {
                        throw new ArgumentException("Invalid linked progressive configuration");
                    }

                    ValidateLinkedProgressiveLevel(level, linkedLevel, clearedErrors, addedErrors);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}