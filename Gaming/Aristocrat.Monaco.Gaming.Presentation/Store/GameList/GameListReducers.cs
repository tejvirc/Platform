namespace Aristocrat.Monaco.Gaming.Presentation.Store.GameList;

using System.Collections.Immutable;
using Aristocrat.Monaco.Gaming.Presentation.Store.Chooser;
using Fluxor;

public static class GameListReducers
{
    [ReducerMethod]
    public static GameListState Loaded(GameListState state, GameListLoadedAction action)
    {
        return state with
        {
            Games = ImmutableList.CreateRange(action.Games)
        };
    }
}
