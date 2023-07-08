namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using System.Collections.Immutable;
using Extensions.Fluxor;
using Models;
using static Extensions.Fluxor.Selectors;

public static class ChooserSelectors
{
    public static readonly ISelector<ChooserState, IImmutableList<GameInfo>> GamesSelector = CreateSelector(
        (ChooserState s) => s.Games);
}