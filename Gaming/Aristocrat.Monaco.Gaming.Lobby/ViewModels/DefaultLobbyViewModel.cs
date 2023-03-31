namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Aristocrat.Monaco.Gaming.Lobby.Models;
using Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;
using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor.Selectors;
using Fluxor;
using Store.Lobby;
using Views;

public class DefaultLobbyViewModel : ObservableObject, IChooserViewTarget
{
    private readonly IDispatcher _dispatcher;
    private readonly IState<LobbyState> _state;

    private string _chooserViewName;

    public DefaultLobbyViewModel(IDispatcher dispatcher, IState<LobbyState> state)
    {
        _dispatcher = dispatcher;
        _state = state;

        _state.StateChanged += OnStateChanged;

        _chooserViewName = ViewNames.Chooser;
    }

    string IChooserViewTarget.ViewName => _chooserViewName;

    private void OnStateChanged(object sender, EventArgs e)
    {

    }
}
