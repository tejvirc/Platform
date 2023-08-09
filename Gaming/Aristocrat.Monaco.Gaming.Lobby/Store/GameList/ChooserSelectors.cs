namespace Aristocrat.Monaco.Gaming.Lobby.Store.GameList;

using System.Collections.Immutable;
using System.Linq;
using Extensions.Fluxor;
using UI.Models;
using static Extensions.Fluxor.Selectors;

public static class GameListSelectors
{
    public static readonly ISelector<GameListState, IImmutableList<GameInfo>> SelectGames = CreateSelector(
        (GameListState state) => state.Games);

    public static readonly ISelector<GameListState, int> SelectGameCount = CreateSelector(
        SelectGames, games => games.Count);

    public static readonly ISelector<GameListState, int> SelectUniqueThemesCount = CreateSelector(
        SelectGames, games => games.Where(g => g.Enabled).Select(o => o.ThemeId).Distinct().Count());
}
