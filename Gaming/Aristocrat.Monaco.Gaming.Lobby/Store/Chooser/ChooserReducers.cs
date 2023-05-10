namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using System.Collections.Immutable;
using Fluxor;

public class ChooserReducers
{
    [ReducerMethod]
    public static ChooserState Reduce(ChooserState state, GamesLoadedAction payload) =>
        state with
        {
            Games = ImmutableList.CreateRange(payload.Games)
        };

    public static ChooserState Reduce(ChooserState state, StartupAction payload)
    {
        return state with
        {
            IsExtraLargeIcons = payload.Configuration.LargeGameIconsEnabled,
            GamesPerPage = payload.Configuration.MaxDisplayedGames
        };
    }
}
