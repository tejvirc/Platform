namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Commands;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Extensions.Fluxor;
using Microsoft.Extensions.Logging;
using UI.Converters;
using UI.Models;
using UI.ViewModels;
using static Store.Chooser.ChooserSelectors;

public class ChooserViewModel : ObservableObject
{
    private readonly ILogger<ChooserViewModel> _logger;

    private readonly SubscriptionList _subscriptions = new();

    private double _componentHeight;
    private double _componentWidth;
    private int _gameCount;
    private int _denomFilter;
    private bool _preserveGameLayoutSideMargins;
    private double _chooseGameOffsetY;
    private bool _isLobbyVisible = true;
    private bool _isIsMultipleGameAssociatedSapLevelTwoEnabled;
    private bool _isExtraLargeGameIconTabActive;
    private bool _isTabView;
    private bool _isResponsibleGamingInfoVisible;

    public ChooserViewModel(ILogger<ChooserViewModel> logger, ISelector selector, IApplicationCommands commands)
    {
        _logger = logger;

        LoadedCommand = new RelayCommand(OnLoaded);
        UnloadedCommand = new RelayCommand(OnUnloaded);
        ShutdownCommand = new RelayCommand(OnShutdown);
        GameSelectCommand = new RelayCommand<GameInfo>(OnGameSelected);
        DenomSelectedCommand = new RelayCommand<DenominationInfoViewModel>(OnDenomSelected);
        ResponsibleGamingDialogOpenCommand = new RelayCommand(OnResponsibleGamingDialogOpen);
        DenominationForSpecificGamePressedCommand = new RelayCommand<object[]?>(OnDenominationForSpecificGamePressed);

        ProgressiveLabelDisplay = new ProgressiveLobbyIndicatorViewModel(GameList);

        commands.ShutdownCommand.RegisterCommand(ShutdownCommand);

        _subscriptions += selector.Select(SelectGames).Subscribe(OnGamesUpdated);
        _subscriptions += selector.Select(SelectChooseGameOffsetY).Subscribe(OnChooseGameOffsetYUpdated);
    }

    public RelayCommand LoadedCommand { get; }

    public RelayCommand UnloadedCommand { get; }

    public RelayCommand ShutdownCommand { get; }

    public RelayCommand<GameInfo> GameSelectCommand { get; }

    public RelayCommand<DenominationInfoViewModel> DenomSelectedCommand { get; }

    public RelayCommand<object[]?> DenominationForSpecificGamePressedCommand { get; }

    public RelayCommand ResponsibleGamingDialogOpenCommand { get; }

    public ObservableCollection<GameInfo> GameList { get; } = new();

    public ObservableCollection<GameInfo> DisplayedGameList { get; } = new();

    public double ComponentHeight
    {
        get => _componentHeight;

        set => SetProperty(ref _componentHeight, value);
    }

    public double ComponentWidth
    {
        get => _componentWidth;

        set => SetProperty(ref _componentWidth, value);
    }

    public int GameCount
    {
        get => _gameCount;

        set => SetProperty(ref _gameCount, value);
    }

    public int DenomFilter
    {
        get => _denomFilter;

        set => SetProperty(ref _denomFilter, value);
    }

    public bool PreserveGameLayoutSideMargins
    {
        get => _preserveGameLayoutSideMargins;

        set => SetProperty(ref _preserveGameLayoutSideMargins, value);
    }

    public double ChooseGameOffsetY
    {
        get => _chooseGameOffsetY;

        set => SetProperty(ref _chooseGameOffsetY, value);
    }

    public bool IsLobbyVisible
    {
        get => _isLobbyVisible;

        set => SetProperty(ref _isLobbyVisible, value);
    }

    public bool IsMultipleGameAssociatedSapLevelTwoEnabled
    {
        get => _isIsMultipleGameAssociatedSapLevelTwoEnabled;

        set => SetProperty(ref _isIsMultipleGameAssociatedSapLevelTwoEnabled, value);

    }

    public bool IsExtraLargeGameIconTabActive
    {
        get => _isExtraLargeGameIconTabActive;

        set => SetProperty(ref _isExtraLargeGameIconTabActive, value);
    }

    public bool IsTabView
    {
        get => _isTabView;

        set => SetProperty(ref _isTabView, value);
    }

    public bool IsResponsibleGamingInfoVisible
    {
        get => _isResponsibleGamingInfoVisible;

        set => SetProperty(ref _isResponsibleGamingInfoVisible, value);
    }

    public ProgressiveLobbyIndicatorViewModel ProgressiveLabelDisplay { get; }

    public GameTabInfoViewModel GameTabInfo { get; } = new GameTabInfoViewModel();

    public GameGridMarginInputs MarginInputs
    {
        get
        {
            var gameCount = DisplayedGameList.Count;
            var gameIconSize = DisplayedGameList.FirstOrDefault()?.GameIconSize ?? Size.Empty;
            var anyVisibleGameHasProgressiveLabel = DisplayedGameList?.Any(x => x.HasProgressiveLabelDisplay) ?? false;
            return new GameGridMarginInputs(
                gameCount,
                IsTabView,
                GameTabInfo.SelectedSubTab?.IsVisible ?? false,
                anyVisibleGameHasProgressiveLabel,
                ComponentHeight,
                IsExtraLargeGameIconTabActive,
                gameIconSize,
                ProgressiveLabelDisplay.MultipleGameAssociatedSapLevelTwoEnabled);
        }
    }

    public ObservableCollection<GameInfo> OrderedGames { get; set; } = new();

    private void OnLoaded()
    {
    }

    private void OnUnloaded()
    {
    }

    private void OnShutdown()
    {
        _subscriptions.UnsubscribeAll();
    }

    private void OnGamesUpdated(IImmutableList<GameInfo> games)
    {
        games.UpdateObservable(GameList, g => g.GameId, (g1, g2) => g1.GameId == g2.GameId);

        DisplayedGameList.Clear();

        foreach (var game in games)
        {
            DisplayedGameList.Add(game);
        }

        GameCount = DisplayedGameList.Count;
    }

    private void OnChooseGameOffsetYUpdated(double offsetY)
    {
        ChooseGameOffsetY = offsetY;
    }

    private void OnDenominationForSpecificGamePressed(object[]? obj)
    {
        if (obj == null)
        {
            return;
        }

        var gameName = (string)obj[0];
        var denom = (long)obj[1];

        LaunchGameWithSpecificDenomination(gameName, denom);
    }

    private void OnGameSelected(GameInfo? game)
    {

    }

    private void OnDenomSelected(DenominationInfoViewModel? denom)
    {

    }

    private void OnResponsibleGamingDialogOpen()
    {
    }

    private void LaunchGameWithSpecificDenomination(string gameName, long denom)
    {
        var selectedGame = GameList.FirstOrDefault(g => g.Name == gameName && g.Denomination == denom);

        _logger.LogDebug("gameId: {GameId}, gameName: {GameName}, denom: {Denom}", selectedGame?.GameId, gameName, denom);

        // LaunchGameFromUi(selectedGame);

        // PlayAudioFile(Sound.Touch);
    }
}
