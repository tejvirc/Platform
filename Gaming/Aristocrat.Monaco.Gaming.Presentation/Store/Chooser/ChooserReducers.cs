namespace Aristocrat.Monaco.Gaming.Presentation.Store.Chooser;

using Fluxor;
using System.Collections.Immutable;
using System.Linq;

public static class ChooserReducers
{
    [ReducerMethod]
    public static ChooserState Reduce(ChooserState state, GameListLoadedAction action)
    {
        return state with
        {
            Games = ImmutableList.CreateRange(action.Games)
        };
    }

    [ReducerMethod]
    public static ChooserState Reduce(ChooserState state, ChooserUpdateTabViewAction action) =>
        state with
        {
            IsTabView = action.IsTabView
        };

    [ReducerMethod]
    public static ChooserState Reduce(ChooserState state, ChooserUpdateDenomFilterAction action) =>
        state with
        {
            DenomFilter = action.DenomFilter
        };
}
