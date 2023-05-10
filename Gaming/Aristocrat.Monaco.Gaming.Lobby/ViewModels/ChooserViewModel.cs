namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor;
using Fluxor.Selectors;
using Models;
using Store.Chooser;

public class ChooserViewModel : ObservableObject
{
    private readonly IState<ChooserState> _state;

    private readonly List<IDisposable> _disposables = new();

    public ChooserViewModel(IStore store, IState<ChooserState> state)
    {
        _state = state;

        _state.StateChanged += OnStateChanged;

        var selector = SelectorFactory.CreateFeatureSelector<ChooserState>();
        var gameSelector = SelectorFactory.CreateSelector(selector, s => s.Games);
        _disposables.Add(store.SubscribeSelector(gameSelector, OnChanged));
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
}
