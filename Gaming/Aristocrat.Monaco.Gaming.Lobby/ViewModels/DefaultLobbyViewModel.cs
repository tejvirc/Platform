namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Aristocrat.Monaco.Gaming.Lobby.Models;
using Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;
using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor.Selectors;
using Fluxor;
using Store.Lobby;

public class DefaultLobbyViewModel : ObservableObject
{
    private readonly IDispatcher _dispatcher;
    private readonly IState<LobbyState> _lobbyState;

    public DefaultLobbyViewModel(IDispatcher dispatcher, IState<LobbyState> lobbyState)
    {
        _dispatcher = dispatcher;
        _lobbyState = lobbyState;

        _lobbyState.StateChanged += LobbyStateOnStateChanged;
    }

    public string Title { get; set; }

    private void LobbyStateOnStateChanged(object sender, EventArgs e)
    {
        Title = _lobbyState.Value.Title;
    }
}
