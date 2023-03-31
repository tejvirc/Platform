namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using Common;
using Fluxor;

public static class LobbyReducers
{
    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, LoadGamesAction action)
    {
        var newState = state.DeepClone();
        newState.IsGamesLoaded = false;
        return newState;
    }

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, GamesLoadedAction action)
    {
        var newState = state.DeepClone();
        newState.IsGamesLoaded = true;
        return newState;
    }
}
