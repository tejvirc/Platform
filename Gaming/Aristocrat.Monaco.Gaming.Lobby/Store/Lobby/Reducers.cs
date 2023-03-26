namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using Common;
using Fluxor;

public static class Reducers
{
    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, LobbyConfigAction payload)
    {
        var newState = state.DeepClone();
        newState.Title = payload.Configuration.UpiTemplate;
        return newState;
    }
}
