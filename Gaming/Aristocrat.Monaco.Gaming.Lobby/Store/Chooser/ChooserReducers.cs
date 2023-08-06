namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using Fluxor;
using System.Collections.Immutable;
using System.Linq;

public static class ChooserReducers
{
    [ReducerMethod]
    public static ChooserState Reduce(ChooserState state, GamesLoadedAction action)
    {
        return state with
        {
            Games = ImmutableList.CreateRange(action.Games)
        };
    }

    [ReducerMethod]
    public static ChooserState Reduce(ChooserState state, UpdateTabViewAction action) =>
        state with
        {
            IsTabView = action.IsTabView
        };

    [ReducerMethod]
    public static ChooserState Reduce(ChooserState state, UpdateDenomFilterAction action) =>
        state with
        {
            DenomFilter = action.DenomFilter
        };
}
