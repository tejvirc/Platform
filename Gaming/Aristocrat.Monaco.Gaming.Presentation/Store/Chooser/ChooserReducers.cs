namespace Aristocrat.Monaco.Gaming.Presentation.Store.Chooser;

using Fluxor;
using System.Collections.Immutable;
using System.Linq;

public static class ChooserReducers
{
    [ReducerMethod]
    public static ChooserState UpdateTabView(ChooserState state, ChooserUpdateTabViewAction action) =>
        state with
        {
            IsTabView = action.IsTabView
        };

    [ReducerMethod]
    public static ChooserState UpdateDenomFilter(ChooserState state, ChooserUpdateDenomFilterAction action) =>
        state with
        {
            DenomFilter = action.DenomFilter
        };
}
