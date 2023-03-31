namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using System.Collections.Immutable;
using Common;
using Fluxor;

public class ChooserReducers
{
    [ReducerMethod]
    public static ChooserState Reduce(ChooserState state, GamesLoadedAction payload) =>
        new() { Games = ImmutableList.CreateRange(payload.Games) };
}
