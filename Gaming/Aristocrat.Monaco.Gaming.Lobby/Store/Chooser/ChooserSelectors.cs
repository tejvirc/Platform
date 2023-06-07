namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using System.Collections.Immutable;
using Models;
using Redux;
using static Redux.Selectors;

public static class ChooserSelectors
{
    public static readonly ISelector<ChooserState, IImmutableList<GameInfo>> GamesSelector = CreateSelector(
        (ChooserState s) => s.Games);

    public static readonly ISelector<ChooserState, bool> IsExtraLargeIconsSelector =
        CreateSelector((ChooserState s) => s.IsExtraLargeIcons);

    public static readonly ISelector<ChooserState, int> GamesPerPageSelector =
        CreateSelector((ChooserState s) => s.GamesPerPage);
}
