namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;
    using Kernel;
    using Models;
    using Progressives;

    public class ProgressiveSummaryViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IGameProvider _gameProvider;
        private readonly IProgressiveConfigurationProvider _progressiveConfiguration;
        private ObservableCollection<ProgressiveSummaryModel> _progressiveSummary;

        public ProgressiveSummaryViewModel()
        {
            var container = ServiceManager.GetInstance().GetService<IContainerService>().Container;

            _gameProvider = container.GetInstance<IGameProvider>();
            _progressiveConfiguration = container.GetInstance<IProgressiveConfigurationProvider>();
        }

        public ObservableCollection<ProgressiveSummaryModel> ProgressiveSummary
        {
            get => _progressiveSummary;
            set
            {
                _progressiveSummary = value;
                RaisePropertyChanged(nameof(ProgressiveSummary));
            }
        }

        public bool HasGames => ProgressiveSummary?.Any() ?? false;

        /// <inheritdoc/>
        protected override void OnLoaded()
        {
            LoadData();
            base.OnLoaded();
        }

        private void LoadData()
        {
            ProgressiveSummary = new ObservableCollection<ProgressiveSummaryModel>();
            foreach (var progressiveLevel in LoadProgressiveSummary())
            {
                var formatLevel = true;
                foreach (var (gameName, winName) in progressiveLevel.ConfiguredGames)
                {
                    ProgressiveSummary.Add(
                        new ProgressiveSummaryModel
                        {
                            ProgressiveLevel = formatLevel ? progressiveLevel.LevelName : string.Empty,
                            CurrentValue = formatLevel ? progressiveLevel.CurrentValue : string.Empty,
                            ConfiguredGame = gameName,
                            WinLevel = winName
                        }
                    );

                    formatLevel = false;
                }
            }

            RaisePropertyChanged(nameof(ProgressiveSummary));
            RaisePropertyChanged(nameof(HasGames));
        }

        private IEnumerable<ProgressiveLevelInfo> LoadProgressiveSummary()
        {
            var levelInfos = new List<ProgressiveLevelInfo>();
            var linkedLevels = _progressiveConfiguration.ViewLinkedProgressiveLevels();
            var sharedSapLevel = _progressiveConfiguration.ViewSharedSapLevels();

            var games = _gameProvider.GetGames().OrderBy(g => g.ThemeName);

            foreach (var (game, denom) in games.SelectMany(
                g => g.Denominations.Where(d => d.Active).Select(d => (Game: g, Denom: d))))
            {
                var enabledLevels = _progressiveConfiguration.ViewProgressiveLevels(game.Id, denom.Value).Where(
                    p => (string.IsNullOrEmpty(p.BetOption) || p.BetOption.Equals(denom.BetOption)) &&
                         p.CurrentState != ProgressiveLevelState.Init);
                foreach (var progressive in enabledLevels)
                {
                    ProgressiveLevelInfo levelInfo;
                    if (progressive.LevelType == ProgressiveLevelType.Selectable)
                    {
                        if (string.IsNullOrEmpty(progressive.AssignedProgressiveId.AssignedProgressiveKey))
                        {
                            continue;
                        }

                        levelInfo =
                            levelInfos.FirstOrDefault(
                                x => x.AssignableKey == progressive.AssignedProgressiveId.AssignedProgressiveKey ||
                                     x.Levels.Contains(progressive)) ??
                            GetProgressiveInformation(progressive, linkedLevels, sharedSapLevel, game.Id);
                    }
                    else
                    {
                        levelInfo = levelInfos.FirstOrDefault(x => x.Levels.Contains(progressive)) ??
                                    GetProgressiveInformation(progressive, linkedLevels, sharedSapLevel, game.Id);
                    }

                    if (progressive.AssignedProgressiveId.AssignedProgressiveType ==
                        AssignableProgressiveType.AssociativeSap)
                    {
                        var updateInfo = levelInfos.FirstOrDefault(i => i.LevelName.Equals(levelInfo.LevelName));
                        if (updateInfo != null)
                        {
                            levelInfo = updateInfo;
                        }
                    }

                    levelInfo.Levels.Add(progressive);
                    levelInfo.ConfiguredGames.Add(
                        ($"{game.ThemeName} {denom.Value.MillicentsToDollars().FormattedCurrencyString()}",
                            progressive.LevelName));
                    if (!levelInfos.Contains(levelInfo))
                    {
                        levelInfos.Add(levelInfo);
                    }
                }
            }

            return levelInfos;
        }

        private ProgressiveLevelInfo GetProgressiveInformation(
            IViewableProgressiveLevel progressiveLevel,
            IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels,
            IEnumerable<IViewableSharedSapLevel> sharedSapLevels,
            int gameId)
        {
            var defaultCurrent = progressiveLevel.CurrentValue.MillicentsToDollars().FormattedCurrencyString(true);
            switch (progressiveLevel.AssignedProgressiveId.AssignedProgressiveType)
            {
                case AssignableProgressiveType.Linked:
                    var linkedLevel = linkedLevels
                        .FirstOrDefault(
                            x => x.LevelName == progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey);

                    return new ProgressiveLevelInfo
                    {
                        LevelName = linkedLevel?.LevelName,
                        AssignableKey = progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey,
                        CurrentValue = linkedLevel?.Amount.CentsToDollars().FormattedCurrencyString(true) ?? defaultCurrent
                    };
                case AssignableProgressiveType.AssociativeSap:
                case AssignableProgressiveType.CustomSap:
                    var sharedSapLevel = sharedSapLevels
                        .FirstOrDefault(
                            x => x.LevelAssignmentKey == progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey);
                    return new ProgressiveLevelInfo
                    {
                        LevelName = sharedSapLevel?.Name,
                        AssignableKey = progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey,
                        CurrentValue = sharedSapLevel?.CurrentValue.MillicentsToDollars().FormattedCurrencyString(true) ?? defaultCurrent
                    };
                default:
                    return new ProgressiveLevelInfo
                    {
                        LevelName = progressiveLevel.LevelName,
                        AssignableKey = progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey,
                        CurrentValue = progressiveLevel.CurrentValue.MillicentsToDollars().FormattedCurrencyString(true)
                    };
            }
        }

        private class ProgressiveLevelInfo
        {
            public string LevelName { get; set; }

            public string AssignableKey { get; set; }

            public string CurrentValue { get; set; }

            public List<(string GameName, string WinName)> ConfiguredGames { get; } = new List<(string, string)>();

            public List<IViewableProgressiveLevel> Levels { get; } = new List<IViewableProgressiveLevel>();
        }
    }
}