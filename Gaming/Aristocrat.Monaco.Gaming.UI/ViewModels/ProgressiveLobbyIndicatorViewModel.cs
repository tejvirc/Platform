namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;
    using Kernel;
    using Localization.Properties;
    using Models;
    using MVVM;
    using MVVM.ViewModel;
    using Progressives;

    public class ProgressiveLobbyIndicatorViewModel : BaseEntityViewModel, IDisposable
    {
        private readonly LobbyViewModel _lobby;
        private readonly IProgressiveConfigurationProvider _progressiveConfiguration;
        private readonly ISharedSapProvider _sharedSap;
        private readonly ILinkedProgressiveProvider _linkedProgressive;
        private readonly IProgressiveErrorProvider _errorProvider;
        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;
        private readonly IProgressiveLevelProvider _progressiveLevelProvider;
        private readonly ISharedSapProvider _sharedSapProvider;

        private bool _disposed;
        private IEnumerable<char> _multipleGameAssociatedSapLevelOneAmount;
        private IEnumerable<char> _multipleGameAssociatedSapLevelTwoAmount;
        private bool _multipleGameAssociatedSapLevelOneEnabled;
        private bool _multipleGameAssociatedSapLevelTwoEnabled;

        public ProgressiveLobbyIndicatorViewModel(LobbyViewModel lobby)
            : this(
                lobby,
                ServiceManager.GetInstance().GetService<IProgressiveConfigurationProvider>(),
                ServiceManager.GetInstance().GetService<ISharedSapProvider>(),
                ServiceManager.GetInstance().GetService<ILinkedProgressiveProvider>(),
                ServiceManager.GetInstance().GetService<IProgressiveErrorProvider>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IGameProvider>(),
                ServiceManager.GetInstance().TryGetService<IProgressiveLevelProvider>(),
                ServiceManager.GetInstance().TryGetService<ISharedSapProvider>())
        {
        }

        public ProgressiveLobbyIndicatorViewModel(
            LobbyViewModel lobby,
            IProgressiveConfigurationProvider progressiveConfiguration,
            ISharedSapProvider sharedSap,
            ILinkedProgressiveProvider linkedProgressive,
            IProgressiveErrorProvider errorProvider,
            IPropertiesManager properties,
            IEventBus eventBus,
            IGameProvider gameProvider,
            IProgressiveLevelProvider progressiveLevelProvider,
            ISharedSapProvider sharedSapProvider)
        {
            _lobby = lobby ?? throw new ArgumentNullException(nameof(lobby));
            _progressiveConfiguration = progressiveConfiguration ?? throw new ArgumentNullException(nameof(progressiveConfiguration));
            _sharedSap = sharedSap ?? throw new ArgumentNullException(nameof(sharedSap));
            _linkedProgressive = linkedProgressive ?? throw new ArgumentNullException(nameof(linkedProgressive));
            _errorProvider = errorProvider ?? throw new ArgumentNullException(nameof(errorProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _progressiveLevelProvider = progressiveLevelProvider ?? throw new ArgumentNullException(nameof(progressiveLevelProvider));
            _sharedSapProvider = sharedSapProvider ?? throw new ArgumentNullException(nameof(sharedSapProvider));

            // We only need to listen to link updates as all others can't change in the lobby
            _eventBus.Subscribe<LinkedProgressiveUpdatedEvent>(this, evt => MvvmHelper.ExecuteOnUI(() => Handler(evt)));
            _eventBus.Subscribe<ProgressiveGameDisabledEvent>(this, evt => MvvmHelper.ExecuteOnUI(() => Handler(evt)));
            _eventBus.Subscribe<ProgressiveGameEnabledEvent>(this, evt => MvvmHelper.ExecuteOnUI(() => Handler(evt)));
            _eventBus.Subscribe<PropertyChangedEvent>(
                this,
                _ => MvvmHelper.ExecuteOnUI(() => UpdateProgressiveIndicator(_lobby.GameList)),
                evt => evt.PropertyName == GamingConstants.ProgressiveLobbyIndicatorType);
        }

        public IEnumerable<char> MultipleGameAssociatedSapLevelOneAmount
        {
            get => _multipleGameAssociatedSapLevelOneAmount;
            set => SetProperty(ref _multipleGameAssociatedSapLevelOneAmount, value);
        }

        public IEnumerable<char> MultipleGameAssociatedSapLevelTwoAmount
        {
            get => _multipleGameAssociatedSapLevelTwoAmount;
            set => SetProperty(ref _multipleGameAssociatedSapLevelTwoAmount, value);
        }

        public bool MultipleGameAssociatedSapLevelOneEnabled
        {
            get => _multipleGameAssociatedSapLevelOneEnabled;
            set => SetProperty(ref _multipleGameAssociatedSapLevelOneEnabled, value);
        }

        public bool MultipleGameAssociatedSapLevelTwoEnabled
        {
            get => _multipleGameAssociatedSapLevelTwoEnabled;
            set => SetProperty(ref _multipleGameAssociatedSapLevelTwoEnabled, value);
        }

        public void UpdateProgressiveIndicator(IEnumerable<GameInfo> games)
        {
            foreach (var game in games)
            {
                var progressiveLevels = _progressiveConfiguration.ViewConfiguredProgressiveLevels(game.GameId, game.Denomination)
                    .Where(x => (x.LevelType == ProgressiveLevelType.Selectable || x.LevelType == ProgressiveLevelType.LP || x.LevelType == ProgressiveLevelType.Sap) &&
                                x.AssignedProgressiveId.AssignedProgressiveType != AssignableProgressiveType.None).ToList();
                _errorProvider.CheckProgressiveLevelErrors(progressiveLevels);
                var gameDetail = _gameProvider.GetGame(game.GameId);
                game.ProgressiveIndicator = GetProgressiveLobbyIndicator(
                    gameDetail.GameIconType,
                    gameDetail,
                    gameDetail.Denominations.Single(d => d.Value == game.Denomination),
                    progressiveLevels);
                UpdateGameProgressiveText(game);
                UpdateGameAssociativeSapText(game);
                UpdateMultipleGameAssociativeSapText();
            }
        }

        public void UpdateGameProgressiveText(GameInfo game)
        {
            if (!game.HasProgressiveLabelDisplay)
            {
                return;
            }

            if (game.ProgressiveErrorVisible)
            {
                game.ProgressiveIndicatorText =
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.ProgressiveLobbyError);
            }
            else if (game.ProgressiveIndicator == ProgressiveLobbyIndicator.ProgressiveLabel)
            {
                game.ProgressiveIndicatorText =
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.ProgressiveLobbyLabel);
            }
            else
            {
                var (levelId, amount) = _progressiveConfiguration.ViewProgressiveLevels(game.GameId, game.Denomination)
                    .Where(x => (string.IsNullOrEmpty(x.BetOption) || x.BetOption == game.BetOption) &&
                                (x.LevelType == ProgressiveLevelType.Selectable ||
                                 x.LevelType == ProgressiveLevelType.LP) &&
                                x.AssignedProgressiveId.AssignedProgressiveType != AssignableProgressiveType.None)
                    .Select(GetCurrentLevelAmounts).OrderByDescending(x => x.CurrentValue).FirstOrDefault();

                game.ProgressiveIndicatorText = levelId != 0 || amount <= 0
                    ? Localizer.For(CultureFor.Player).GetString(ResourceKeys.ProgressiveLobbyLabel)
                    : amount.FormattedCurrencyString();
            }
        }

        public void UpdateGameAssociativeSapText(GameInfo game)
        {
            // Currently, lobby display for associated sap levels apply only to games in the LightningLink category
            if (!game.HasProgressiveLabelDisplay || game.Category != GameCategory.LightningLink)
            {
                return;
            }

            if (game.ProgressiveErrorVisible)
            {
                game.ProgressiveIndicatorText =
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.ProgressiveLobbyError);
            }
            else if (game.ProgressiveIndicator == ProgressiveLobbyIndicator.ProgressiveLabel)
            {
                game.ProgressiveIndicatorText =
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.ProgressiveLobbyLabel);
            }
            else
            {
                // The DisplayMeterName comes from the GSA manifest, and will contain the name of the associated sap progressive levels
                // this game is associated with
                var gameDetail = _properties.GetValues<IGameDetail>(GamingConstants.Games).SingleOrDefault(g => g.Id == game.GameId);
                if (string.IsNullOrEmpty(gameDetail?.DisplayMeterName))
                {
                    return;
                }

                // The DisplayMeterName will be in a format like: WinnersWorldProgressive_BigFortuneMajor, the level name is BigFortuneMajor
                var levelName = gameDetail.DisplayMeterName.Split('_').Last();

                var (_, amount) = _progressiveConfiguration.ViewProgressiveLevels(game.GameId, game.Denomination)
                    .Where(x => x.LevelName == levelName &&
                           x.LevelType == ProgressiveLevelType.Sap &&
                           x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.AssociativeSap)
                    .Select(GetCurrentLevelAmounts).OrderByDescending(x => x.CurrentValue).FirstOrDefault();

                game.ProgressiveIndicatorText = amount <= 0
                    ? Localizer.For(CultureFor.Player).GetString(ResourceKeys.ProgressiveLobbyLabel)
                    : amount.FormattedCurrencyString();
            }
        }

        public void UpdateMultipleGameAssociativeSapText()
        {
            Dictionary<string, (int, string)> levelUpdates = new Dictionary<string, (int, string)>();
            var gameDetails = _properties.GetValues<IGameDetail>(GamingConstants.Games).ToList();

            foreach (var game in _lobby.GameList)
            {
                // Currently, lobby display for associated sap levels apply only to games in the LightningLink category
                if (game.Category != GameCategory.LightningLink)
                {
                    continue;
                }

                var gameDetail = gameDetails.SingleOrDefault(g => g.Id == game.GameId);
                if (gameDetail is null || gameDetail.AssociatedSapDisplayMeterName == null)
                {
                    continue;
                }

                // The order of the AssociatedSapDisplayMeterName must be from largest(grand) to smallest
                for (int lvIdx = 0; lvIdx < gameDetail.AssociatedSapDisplayMeterName.Count(); lvIdx++)
                {
                    var gameDetailAsapName = gameDetail.AssociatedSapDisplayMeterName.ElementAt(lvIdx);
                    foreach (var sharedLevel in _sharedSapProvider.ViewSharedSapLevels())
                    {
                        // This level is shared across multiple games
                        if (sharedLevel.Name == gameDetailAsapName)
                        {
                            if (game.ProgressiveErrorVisible)
                            {
                                levelUpdates[sharedLevel.LevelAssignmentKey] = (lvIdx,
                                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.ProgressiveLobbyError));
                            }
                            else if (game.ProgressiveIndicator == ProgressiveLobbyIndicator.ProgressiveLabel)
                            {
                                levelUpdates[sharedLevel.LevelAssignmentKey] = (lvIdx,
                                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.ProgressiveLobbyLabel));
                            }
                            else
                            {
                                levelUpdates[sharedLevel.LevelAssignmentKey] = sharedLevel.CurrentValue <= 0
                                    ? (lvIdx, Localizer.For(CultureFor.Player).GetString(ResourceKeys.ProgressiveLobbyLabel))
                                    : (lvIdx, sharedLevel.CurrentValue.MillicentsToDollarsNoFraction().FormattedCurrencyString());
                            }
                        }
                    }
                }
            }

            MultipleGameAssociatedSapLevelOneEnabled = false;
            MultipleGameAssociatedSapLevelTwoEnabled = false;

            if (levelUpdates.Any())
            {
                var updatesInOrder = levelUpdates.Values.ToDictionary(x => x.Item1, x => x.Item2);

                // levelUpdates[0] will be the grand amount
                MultipleGameAssociatedSapLevelOneAmount = updatesInOrder[0].ToList();
                MultipleGameAssociatedSapLevelOneEnabled = true;

                if (updatesInOrder.ContainsKey(1))
                {
                    // levelUpdates[1] will be the major amount (if shared across games)
                    MultipleGameAssociatedSapLevelTwoAmount = updatesInOrder[1].ToList();
                    MultipleGameAssociatedSapLevelTwoEnabled = true;
                }
            }
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

        private ProgressiveLobbyIndicator GetProgressiveLobbyIndicator(
            GameIconType iconType,
            IGameDetail game,
            IDenomination denom,
            IEnumerable<IViewableProgressiveLevel> progressiveLevels)
        {
            var lobbyIndicator = _properties.GetValue(
                GamingConstants.ProgressiveLobbyIndicatorType,
                ProgressiveLobbyIndicator.Disabled);
            if (lobbyIndicator == ProgressiveLobbyIndicator.Disabled || !progressiveLevels.Any(
                x => x.GameId == game.Id && x.Denomination.Contains(denom.Value) &&
                     (string.IsNullOrEmpty(x.BetOption) || x.BetOption == denom.BetOption)))
            {
                return ProgressiveLobbyIndicator.Disabled;
            }

            switch (iconType)
            {
                case GameIconType.Default:
                    return lobbyIndicator;
                case GameIconType.NoProgressiveInformation:
                    return ProgressiveLobbyIndicator.Disabled;
                case GameIconType.ProgressiveValue:
                    return ProgressiveLobbyIndicator.ProgressiveValue;
                case GameIconType.ProgressiveLabel:
                    return ProgressiveLobbyIndicator.ProgressiveLabel;
                default:
                    throw new ArgumentOutOfRangeException(nameof(iconType), iconType, null);
            }
        }

        private void Handler(ProgressiveGameEnabledEvent evt)
        {
            var gameInfo = _lobby.GameList.FirstOrDefault(
                x => x.GameId == evt.GameId && x.Denomination == evt.Denom &&
                     (string.IsNullOrEmpty(evt.BetOption) || x.BetOption == evt.BetOption));
            if (gameInfo is null)
            {
                return;
            }

            gameInfo.ProgressiveErrorVisible = false;
            UpdateGameProgressiveText(gameInfo);
            UpdateGameAssociativeSapText(gameInfo);
            UpdateMultipleGameAssociativeSapText();
        }

        private void Handler(ProgressiveGameDisabledEvent evt)
        {
            var gameInfo = _lobby.GameList.FirstOrDefault(
                x => x.GameId == evt.GameId && x.Denomination == evt.Denom &&
                     (string.IsNullOrEmpty(evt.BetOption) || evt.BetOption == x.BetOption));
            if (gameInfo is null)
            {
                return;
            }

            gameInfo.ProgressiveErrorVisible = true;
            UpdateGameProgressiveText(gameInfo);
            UpdateGameAssociativeSapText(gameInfo);
            UpdateMultipleGameAssociativeSapText();
        }

        private void Handler(LinkedProgressiveUpdatedEvent evt)
        {
            var progressiveLevels = _progressiveConfiguration.ViewProgressiveLevels();
            var updatedGames = progressiveLevels.Where(
                    x => x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                         evt.LinkedProgressiveLevels.Any(
                             l => l.LevelName == x.AssignedProgressiveId.AssignedProgressiveKey))
                .SelectMany(
                    l => l.Denomination.Select(
                        x => _lobby.GameList.FirstOrDefault(
                            g => g.GameId == l.GameId && g.Denomination == x &&
                                 (string.IsNullOrEmpty(l.BetOption) || l.BetOption == g.BetOption))))
                .Where(x => x != null).Distinct();
            foreach (var game in updatedGames)
            {
                UpdateGameProgressiveText(game);
            }
        }

        private (int LevelId, decimal CurrentValue) GetCurrentLevelAmounts(IViewableProgressiveLevel level)
        {
            if (level.LevelType != ProgressiveLevelType.LP && level.LevelType != ProgressiveLevelType.Selectable)
            {
                return (level.LevelId, level.CurrentValue.MillicentsToDollarsNoFraction());
            }

            switch (level.AssignedProgressiveId.AssignedProgressiveType)
            {
                case AssignableProgressiveType.None:
                    return (level.LevelId, -1);
                case AssignableProgressiveType.AssociativeSap:
                case AssignableProgressiveType.CustomSap:
                    return (level.LevelId,
                        _sharedSap.ViewSharedSapLevel(
                            level.AssignedProgressiveId.AssignedProgressiveKey,
                            out var sharedSap)
                            ? sharedSap.CurrentValue.MillicentsToDollarsNoFraction()
                            : -1);
                case AssignableProgressiveType.Linked:
                    return (level.LevelId,
                        _linkedProgressive.ViewLinkedProgressiveLevel(
                            level.AssignedProgressiveId.AssignedProgressiveKey,
                            out var linked)
                            ? linked.Amount.CentsToDollars()
                            : -1);
            }

            return (level.LevelId, level.CurrentValue.MillicentsToDollarsNoFraction());
        }
    }
}