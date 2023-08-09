namespace Aristocrat.Monaco.Gaming.Lobby.Store.GameList;

using System.Collections.Immutable;
using Fluxor;

public static class GameListReducers
{
    [ReducerMethod]
    public static GameListState Reduce(GameListState state, GamesLoadedAction action)
    {
        return state with
        {
            Games = ImmutableList.CreateRange(action.Games)
        };
    }
}
