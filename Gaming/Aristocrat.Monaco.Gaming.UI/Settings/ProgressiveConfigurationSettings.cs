namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts.Settings;
    using Contracts;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;
    using Kernel;
    using Progressives;

    /// <summary>
    ///     Progressive configuration settings provider.
    /// </summary>
    public sealed class ProgressiveConfigurationSettings : IConfigurationSettings
    {
        private ISharedSapProvider _sharedSapProvider;
        private ILinkedProgressiveProvider _linkedProgressiveProvider;
        private IProgressiveConfigurationProvider _progressiveConfigurationProvider;
        private IGameProvider _gameProvider;

        /// <inheritdoc />
        public string Name => "Progressive";

        /// <inheritdoc />
        public ConfigurationGroup Groups => ConfigurationGroup.Game;

        /// <inheritdoc />
        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task Apply(ConfigurationGroup configGroup, object settings)
        {
            if (!Groups.HasFlag(configGroup))
            {
                throw new ArgumentOutOfRangeException(nameof(configGroup));
            }

            if (!(settings is ProgressiveSettings progressiveSettings))
            {
                throw new ArgumentException($@"Invalid settings type, {settings?.GetType()}", nameof(settings));
            }

            return ApplySettings(progressiveSettings);
        }

        /// <inheritdoc />
        public Task<object> Get(ConfigurationGroup configGroup)
        {
            if (configGroup != ConfigurationGroup.Game)
            {
                throw new ArgumentOutOfRangeException(nameof(configGroup), configGroup, null);
            }

            return GetProgressiveSettings();
        }

        private static ProgressiveLevelSettings CreateProgressiveLevelSettings(
            IViewableProgressiveLevel level,
            IEnumerable<IGameDetail> games)
        {
            return games.FirstOrDefault(game => game.Id == level.GameId) is IGameDetail foundGame
                ? new ProgressiveLevelSettings(foundGame.PaytableId, foundGame.ThemeId, level)
                : new ProgressiveLevelSettings(level);
        }

        private static bool IsNonSapValid(IViewableProgressiveLevel level)
        {
            return level != null
                   && (level.LevelType == ProgressiveLevelType.Selectable
                       || level.LevelType == ProgressiveLevelType.LP)
                   && level.AssignedProgressiveId != null
                   && level.AssignedProgressiveId.AssignedProgressiveType != AssignableProgressiveType.None
                   && !string.IsNullOrWhiteSpace(level.AssignedProgressiveId.AssignedProgressiveKey);
        }

        private static bool CanUseProgressiveLevel(IViewableProgressiveLevel level)
        {
            return level.LevelType != ProgressiveLevelType.Unknown
                   && level.CurrentState != ProgressiveLevelState.Init
                   && !string.IsNullOrWhiteSpace(level.LevelName)
                   && IsNonSapValid(level);
        }

        private static ProgressiveLevelAssignment CreateProgressiveLevelAssignment(
            IViewableProgressiveLevel level,
            IGameDetail game,
            IDenomination denomination)
        {
            return new ProgressiveLevelAssignment(
                game,
                denomination.Value,
                level,
                level.AssignedProgressiveId,
                level.CurrentValue);
        }

        private Task ApplySettings(ProgressiveSettings settings)
        {
            SetupProviders();

            var sharedSapLevels = settings.CustomSapLevels ?? Enumerable.Empty<ProgressiveSharedLevelSettings>();
            var games = _gameProvider.GetAllGames()
                .SelectMany(g => g.Denominations.Where(d => d.Active).Select(d => (game: g, denom: d)));

            var currentSapLevels = _sharedSapProvider.ViewSharedSapLevels();
            var customSap = sharedSapLevels.Select(
                setting =>
                {
                    var sapLevel = setting.ToSharedSapLevel();

                    return (
                        currentSapLevels.FirstOrDefault(level => level.Name == setting.Name) ?? _sharedSapProvider
                            .AddSharedSapLevel(new List<IViewableSharedSapLevel> { sapLevel }).First(), setting);
                }).ToList();
            var linkedProgressiveLevels = _linkedProgressiveProvider.ViewLinkedProgressiveLevels();

            _progressiveConfigurationProvider.AssignLevelsToGame(
                GetAssignedProgressiveLevels(settings, games, customSap, linkedProgressiveLevels).ToList());

            return Task.CompletedTask;
        }

        private IEnumerable<ProgressiveLevelAssignment> GetAssignedProgressiveLevels(
            ProgressiveSettings settings,
            IEnumerable<(IGameDetail game, IDenomination denom)> games,
            IReadOnlyCollection<(IViewableSharedSapLevel, ProgressiveSharedLevelSettings)> customSap,
            IReadOnlyCollection<IViewableLinkedProgressiveLevel> linkedProgressiveLevels)
        {
            return games.SelectMany(
                x =>
                {
                    var levels = settings
                        .CreateProgressiveLevels(
                            customSap,
                            linkedProgressiveLevels,
                            _progressiveConfigurationProvider,
                            x.game,
                            x.denom.Value,
                            x.denom.BetOption).ToList();
                    var levelsToAdd = levels.Any()
                        ? levels
                        // Add these levels if we don't have any configured levels, but the denom is active.
                        // This would occur in denom that only has Non-Progressive's assigned.
                        : _progressiveConfigurationProvider.ViewProgressiveLevels(x.game.Id, x.denom.Value);
                    return levelsToAdd.Select(level => CreateProgressiveLevelAssignment(level, x.game, x.denom));
                });
        }

        private Task<object> GetProgressiveSettings()
        {
            SetupProviders();

            var assignedLevels = GetAssignedProgressiveLevels();
            return Task.FromResult<object>(
                new ProgressiveSettings
                {
                    CustomSapLevels =
                        _sharedSapProvider.ViewSharedSapLevels()
                            .Where(
                                sap => assignedLevels
                                    .Any(
                                        level => level.LevelType == ProgressiveLevelType.Selectable
                                                 && level.AssignedProgressiveId.AssignedProgressiveKey ==
                                                 sap.LevelAssignmentKey))
                            .Select(level => new ProgressiveSharedLevelSettings(level))
                            .ToList(),
                    LinkedProgressiveLevelNames =
                        _linkedProgressiveProvider.ViewLinkedProgressiveLevels().Select(level => level.LevelName)
                            .ToList(),
                    AssignedProgressiveLevels = assignedLevels.ToList()
                });
        }

        private IEnumerable<ProgressiveLevelSettings> GetAssignedProgressiveLevels()
        {
            SetupProviders();

            var games = _gameProvider.GetAllGames();

            return _progressiveConfigurationProvider.ViewProgressiveLevels()
                .Where(CanUseProgressiveLevel)
                .Select(level => CreateProgressiveLevelSettings(level, games));
        }

        private void SetupProviders()
        {
            _sharedSapProvider = _sharedSapProvider ?? ServiceManager.GetInstance().GetService<ISharedSapProvider>();
            _linkedProgressiveProvider = _linkedProgressiveProvider ??
                                         ServiceManager.GetInstance().GetService<ILinkedProgressiveProvider>();
            _progressiveConfigurationProvider = _progressiveConfigurationProvider ??
                                                ServiceManager.GetInstance()
                                                    .GetService<IProgressiveConfigurationProvider>();
            _gameProvider = _gameProvider ?? ServiceManager.GetInstance().GetService<IGameProvider>();
        }
    }
}