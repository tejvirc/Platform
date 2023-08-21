namespace Aristocrat.Monaco.Gaming.Presentation.Store.GameList;

using System.Collections.Immutable;
using Fluxor;

public static class GameListReducers
{
    [ReducerMethod]
    public static GameListState Reduce(GameListState state, GameListLoadedAction action)
    {
        return state with
        {
            Games = ImmutableList.CreateRange(action.Games)
        };
    }
}
