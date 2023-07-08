namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using Fluxor;
using System.Collections.Immutable;
using System.Linq;

public static class ChooserReducers
{
    [ReducerMethod]
    public static ChooserState Reduce(ChooserState state, GamesLoadedAction payload)
    {
        var themesCount = payload.Games.Where(g => g.Enabled).Select(o => o.ThemeId).Distinct().Count();

        return state with
        {
            Games = ImmutableList.CreateRange(payload.Games),
            UniqueThemesCount = themesCount,
            IsSingleGame = themesCount <= 1 && state.AllowGameInCharge
        };
    }
}
