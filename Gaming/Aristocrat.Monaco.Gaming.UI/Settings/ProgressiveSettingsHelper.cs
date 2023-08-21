namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;
    using Localization.Properties;
    using log4net;
    using Progressives;

    internal static class ProgressiveSettingsHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ProgressiveLevel));

        /// <summary>
        ///     Creates progressive levels and provide a message key if an error occurs.
        /// </summary>
        /// <param name="settings">The sap levels to provide.</param>
        /// <param name="sharedSapLevels">The sap levels to provide.</param>
        /// <param name="linkedProgressiveLevels">The linked levels to provide.</param>
        /// <param name="progressiveConfigurationProvider">Gathers all the progressives.</param>
        /// <param name="gameDetail">Allows us to search against levels that are not apart of the provided game.</param>
        /// <param name="denom">The provided denom to filter the levels.</param>
        /// <param name="betOption">The provided betOption to filter the levels.</param>
        /// <returns>A readonly collection of levels with an error message key if an error occurs.</returns>
        public static (string ErrorMessageKey, IReadOnlyCollection<ProgressiveLevel> Levels)
            TryCreateProgressiveLevels(
                this ProgressiveSettings settings,
                IReadOnlyCollection<(IViewableSharedSapLevel assigningLevel, ProgressiveSharedLevelSettings settings)> sharedSapLevels,
                IReadOnlyCollection<IViewableLinkedProgressiveLevel> linkedProgressiveLevels,
                IProgressiveConfigurationProvider progressiveConfigurationProvider,
                IGameDetail gameDetail,
                long denom,
                string betOption)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var settingLevels = new List<ProgressiveLevel>();
            if (gameDetail is null)
            {
                return (string.Empty, settingLevels);
            }

            var linkedProgressives = linkedProgressiveLevels?.Where(
                                         linkedLevel =>
                                             settings.LinkedProgressiveLevelNames?.Any(
                                                 linkedName => linkedLevel.LevelName == linkedName) ?? false)
                                     ?? Enumerable.Empty<IViewableLinkedProgressiveLevel>();

            var assignedLevels = settings.AssignedProgressiveLevels
                ?.Select(
                    level => (level, assignment: GetLevelAssignment(
                        level,
                        gameDetail,
                        denom,
                        betOption,
                        sharedSapLevels,
                        linkedProgressives)))
                .Where(x => x.assignment != null)
                .ToList();

            if (assignedLevels is null || assignedLevels.Count == 0)
            {
                return (string.Empty, settingLevels);
            }

            var packageName = assignedLevels
                .Select(x => x.level.ProgressivePackName)
                .Distinct()
                .Single();

            var matchingExistingLevels = progressiveConfigurationProvider
                ?.ViewProgressiveLevels(gameDetail.Id, denom, packageName).Where(
                    p => string.IsNullOrEmpty(p.BetOption) || string.Equals(
                        p.BetOption,
                        betOption))
                .Cast<ProgressiveLevel>()
                .ToList() ?? new List<ProgressiveLevel>();

            foreach (var (level, assignment) in assignedLevels)
            {
                var (messageKey, createdLevel) = CreateLevelFromSettings(gameDetail, matchingExistingLevels, level, assignment);
                if (!string.IsNullOrEmpty(messageKey))
                {
                    return (messageKey, new List<ProgressiveLevel>());
                }

                if (createdLevel != null)
                {
                    settingLevels.Add(createdLevel);
                }
            }

            var results = matchingExistingLevels.Select(
                existingLevel => settingLevels.FirstOrDefault(l => existingLevel.LevelName == l.LevelName) ??
                                 existingLevel).ToList();

            return (string.Empty, results);
        }

        /// <summary>
        ///     Creates progressive levels with the provided progressive levels.
        /// </summary>
        /// <param name="settings">The sap levels to provide.</param>
        /// <param name="sharedSapLevels">The sap levels to provide.</param>
        /// <param name="linkedProgressiveLevels">The linked levels to provide.</param>
        /// <param name="progressiveConfigurationProvider">Gathers all the progressives.</param>
        /// <param name="gameDetail">Allows us to search against levels that are not apart of the provided game.</param>
        /// <param name="denom">The provided denom to filter the levels.</param>
        /// <param name="betOption">The provided betOption to filter the levels.</param>
        /// <returns>Creates a readonly collection of progressive levels.</returns>
        public static IReadOnlyCollection<ProgressiveLevel> CreateProgressiveLevels(
            this ProgressiveSettings settings,
            IReadOnlyCollection<(IViewableSharedSapLevel assigningLevel, ProgressiveSharedLevelSettings settings)> sharedSapLevels,
            IReadOnlyCollection<IViewableLinkedProgressiveLevel> linkedProgressiveLevels,
            IProgressiveConfigurationProvider progressiveConfigurationProvider,
            IGameDetail gameDetail,
            long denom,
            string betOption)
        {
            return TryCreateProgressiveLevels(
                    settings,
                    sharedSapLevels,
                    linkedProgressiveLevels,
                    progressiveConfigurationProvider,
                    gameDetail,
                    denom,
                    betOption)
                .Levels;
        }

        /// <summary>
        ///     Creates a progressive level from a progressive level setting.
        /// </summary>
        /// <param name="settings">The settings to create the progressive level.</param>
        /// <param name="gameId">Sets the progressive level's game id.</param>
        /// <param name="currentValue">Sets the progressive level's current value.</param>
        /// <param name="currentState">Sets the progressive level's current state.</param>
        /// <param name="levelId">Sets the progressive level's level id.</param>
        /// <param name="assignableId">The assignableId for this level</param>
        /// <returns>Creates a progressive level.</returns>
        public static ProgressiveLevel ToProgressiveLevel(
            this ProgressiveLevelSettings settings,
            int gameId,
            long currentValue,
            ProgressiveLevelState currentState,
            int levelId,
            AssignableProgressiveId assignableId)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return new ProgressiveLevel
            {
                ProgressivePackName = settings.ProgressivePackName,
                ProgressivePackId = settings.ProgressivePackId,
                ProgressiveId = settings.ProgressiveId,
                Denomination = settings.Denomination?.ToList() ?? Enumerable.Empty<long>(),
                BetOption = settings.BetOption,
                GameId = gameId,
                Variation = settings.Variation,
                ProgressivePackRtp = settings.ProgressivePackRtp,
                LevelType = settings.LevelType,
                FundingType = settings.FundingType,
                LevelName = settings.LevelName,
                IncrementRate = settings.IncrementRate,
                HiddenIncrementRate = settings.HiddenIncrementRate,
                Probability = settings.Probability,
                MaximumValue = settings.MaximumValue,
                ResetValue = settings.ResetValue,
                LevelId = levelId,
                LevelRtp = settings.LevelRtp,
                LineGroup = settings.LineGroup,
                AllowTruncation = settings.AllowTruncation,
                Turnover = settings.Turnover,
                TriggerControl = settings.TriggerControl,
                CurrentState = currentState,
                CurrentValue = currentValue,
                AssignedProgressiveId = assignableId ?? settings.AssignedProgressiveId,
                CanEdit = true
            };
        }

        public static (string errorMessageKey, IViewableSharedSapLevel) TryCreateSharedSapLevel(
            this ProgressiveSharedLevelSettings settings,
            IEnumerable<IViewableSharedSapLevel> sharedSap)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (sharedSap == null)
            {
                throw new ArgumentNullException(nameof(sharedSap));
            }

            var existingLevel = sharedSap.FirstOrDefault(level => level.Name == settings.Name);
            if (existingLevel == null ||
                existingLevel.IncrementRate == settings.IncrementRate &&
                existingLevel.MaximumValue == settings.MaximumValue &&
                existingLevel.ResetValue == settings.ResetValue)
            {
                return (string.Empty, existingLevel ?? settings.ToSharedSapLevel());
            }

            Logger.Debug($"Can't import the level {settings.Name} as it would have needed to created a duplicate level name.");
            return (ResourceKeys.CreateCustomSapProgressiveLevelIdError, null);

        }

        /// <summary>
        ///     Creates a shared sap level from our settings.
        /// </summary>
        /// <param name="settings">The settings to create the shared sap level.</param>
        /// <returns>Creates a shared sap level.</returns>
        public static SharedSapLevel ToSharedSapLevel(this ProgressiveSharedLevelSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return new SharedSapLevel
            {
                Id = settings.Id,
                Name = settings.Name,
                SupportedGameTypes = settings.SupportedGameTypes?.ToArray() ?? Enumerable.Empty<GameType>(),
                InitialValue = settings.InitialValue,
                ResetValue = settings.ResetValue,
                IncrementRate = settings.IncrementRate,
                LevelId = 0,
                MaximumValue = settings.MaximumValue,
                CurrentValue = settings.InitialValue,
                CanEdit = true,
                AutoGenerated = settings.AutoGenerated,
                HiddenIncrementRate = settings.HiddenIncrementRate
            };
        }

        private static (string ErrorMessageKey, ProgressiveLevel Level) CreateLevelFromSettings(
            IGameDetail game,
            IEnumerable<ProgressiveLevel> matchingExistingLevels,
            ProgressiveLevelSettings level,
            AssignableProgressiveId assignableId)
        {
            if (level is null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            if (game is null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            var levels = matchingExistingLevels
                ?.Where(
                    existingLevel => existingLevel.LevelName == level.LevelName
                                     && (string.IsNullOrWhiteSpace(level.BetOption)
                                         || existingLevel.BetOption == level.BetOption))
                .Select(existingLevel => existingLevel.LevelId).ToList() ?? new List<int>();

            // Do not import/export SAP levels 
            if (levels.Count == 1 && level.LevelType != ProgressiveLevelType.Sap)
            {
                return (string.Empty,
                    level.ToProgressiveLevel(
                        game.Id,
                        game.InitialValue,
                        ProgressiveLevelState.Ready,
                        levels[0],
                        assignableId));
            }

            if (levels.Count == 0)
            {
                return (string.Empty, null);
            }

            Logger.Error($"{nameof(CreateLevelFromSettings)}: More than one level id was found for level: {level.LevelName}");
            return (ResourceKeys.CreateProgressiveLevelIdError, null);
        }

        private static AssignableProgressiveId GetLevelAssignment(
            ProgressiveLevelSettings level,
            IGameDetail gameDetail,
            long denom,
            string betOption,
            IEnumerable<(IViewableSharedSapLevel level, ProgressiveSharedLevelSettings setting)> sapLevels,
            IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels)
        {
            if (gameDetail == null || (!level?.Denomination.Contains(denom) ?? true) ||
                gameDetail.ThemeId != level.ThemeId || gameDetail.PaytableId != level.PaytableId ||
                !string.IsNullOrEmpty(level.BetOption) && !string.Equals(level.BetOption, betOption))
            {
                return null;
            }

            if (level.LevelType == ProgressiveLevelType.Sap)
            {
                return new AssignableProgressiveId(AssignableProgressiveType.None, string.Empty);
            }

            switch (level.AssignedProgressiveId.AssignedProgressiveType)
            {
                case AssignableProgressiveType.AssociativeSap:
                case AssignableProgressiveType.CustomSap:
                    return sapLevels
                        .Select(
                            x => (
                                assignment: new AssignableProgressiveId(
                                    level.AssignedProgressiveId.AssignedProgressiveType,
                                    x.level.LevelAssignmentKey), x.setting)).FirstOrDefault(
                            x => x.setting.Id.ToString("B") == level.AssignedProgressiveId.AssignedProgressiveKey)
                        .assignment;
                case AssignableProgressiveType.Linked:
                    return linkedLevels
                        .Select(x => new AssignableProgressiveId(AssignableProgressiveType.Linked, x.LevelName))
                        .FirstOrDefault(
                            x => x.AssignedProgressiveKey == level.AssignedProgressiveId.AssignedProgressiveKey);
                default:
                    return null;
            }
        }
    }
}