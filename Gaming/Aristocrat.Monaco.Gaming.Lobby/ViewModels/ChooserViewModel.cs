namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Commands;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls;
using Models;
using Aristocrat.Fluxor.Extensions;
using static Store.Lobby.LobbySelectors;

public class ChooserViewModel : ObservableObject
{
    private readonly SubscriptionList _subscriptions = new();

    private int _gameCount;
    private int _denomFilter;
    private bool _preserveGameLayoutSideMargins;
    private double _chooseGameOffsetY;
    private bool _isLobbyVisible = true;
    private bool _isIsMultipleGameAssociatedSapLevelTwoEnabled;

    public ChooserViewModel(ISelector selector, IApplicationCommands commands)
    {
        LoadedCommand = new RelayCommand(OnLoaded);
        UnloadedCommand = new RelayCommand(OnUnloaded);
        ShutdownCommand = new RelayCommand(OnShutdown);
        GameSelectCommand = new RelayCommand<GameInfo>(OnGameSelected);
        DenomSelectedCommand = new RelayCommand<DenomInfo>(OnDenomSelected);

        commands.ShutdownCommand.RegisterCommand(ShutdownCommand);

        _subscriptions += selector.Select(GamesSelector).Subscribe(
            games =>
            {
                games.UpdateObservable(Games, g => g.GameId, (g1, g2) => g1.GameId == g2.GameId);
            });
    }

    public RelayCommand LoadedCommand { get; }

    public RelayCommand UnloadedCommand { get; }

    public RelayCommand ShutdownCommand { get; }

    public RelayCommand<GameInfo> GameSelectCommand { get; }

    public RelayCommand<DenomInfo> DenomSelectedCommand { get; }

    public ObservableCollection<GameInfo> Games { get; } = new();

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

    public GameGridMarginInputs MarginInputs
    {
        get
        {
            var gameCount = Games?.Count ?? 0;
            var (rows, cols) = GameRowColumnCalculator.CalculateRowColCount(gameCount);
            return new GameGridMarginInputs(
                gameCount,
                false,
                false,
                Games?.Reverse().Take(rows <= 0 ? 0 : gameCount - ((rows - 1) * cols))
                    .Any(x => x.HasProgressiveLabelDisplay) ?? false,
                1080,
                false,
                Games?.FirstOrDefault()?.GameIconSize ?? Size.Empty,
                false,
                Games?.Any(g => g.HasProgressiveLabelDisplay) ?? false);
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

    private void OnGameSelected(GameInfo? game)
    {

    }

    private void OnDenomSelected(DenomInfo? denom)
    {

    }
}
