namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls;
using Fluxor;
using Fluxor.Selectors;
using Models;
using Redux;
using Store.Chooser;

public class ChooserViewModel : ObservableObject
{
    //private readonly IState<ChooserState> _state;
    //private readonly IStateSelectors<ChooserSelectors> _selectors;

    private readonly List<IDisposable> _disposables = new();

    private int _gameCount;
    private int _denomFilter;
    private bool _preserveGameLayoutSideMargins;
    private double _chooseGameOffsetY;
    private bool _isLobbyVisible = true;
    private bool _isIsMultipleGameAssociatedSapLevelTwoEnabled;

    public ChooserViewModel(IStore store, ChooserSelectors selectors)
    {
        GameSelectCommand = new RelayCommand<DenomInfo>(OnDenomSelected);
        DenomSelectedCommand = new RelayCommand<DenomInfo>(OnDenomSelected);

        store.Select(selectors.GamesSelector).Subscribe(
            games =>
            {
                games.UpdateObservable(Games);
            });
    }

    //public ChooserViewModel(IStore store, IState<ChooserState> state, IStateSelectors<ChooserSelectors> selectors)
    //{
    //    _state = state;
    //    _selectors = selectors;

    //    _state.StateChanged += OnStateChanged;
    //    var selector = SelectorFactory.CreateFeatureSelector<ChooserState>();
    //    var gameSelector = SelectorFactory.CreateSelector(selector, s => s.Games);
    //    _disposables.Add(store.SubscribeSelector(gameSelector, OnChanged));

    //    _disposables.Add(selectors.Selectors.GamesSelector.Subscribe(Observer.Create<IImmutableList<GameInfo>>(OnGamesChanged)));

    //    GameSelectCommand = new RelayCommand<DenomInfo>(OnDenomSelected);
    //    DenomSelectedCommand = new RelayCommand<DenomInfo>(OnDenomSelected);
    //}

    public void OnGamesChanged(IImmutableList<GameInfo> games)
    {

    }

    public RelayCommand<DenomInfo> GameSelectCommand { get; }

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

    private void OnChanged(IImmutableList<GameInfo>? games)
    {
        if (games == null)
        {
            return;
        }

        OrderedGames = new ObservableCollection<GameInfo>(games);
    }

    public ObservableCollection<GameInfo> OrderedGames { get; set; } = new();

    private void OnStateChanged(object? sender, EventArgs e)
    {
        // OrderedGames = new ObservableCollection<GameInfo>(_state.Value.Games);
    }

    private void OnDenomSelected(DenomInfo? obj)
    {
    }
}
