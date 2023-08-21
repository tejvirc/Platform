namespace Aristocrat.Monaco.Gaming.Presentation.Store.Chooser;

using System.Collections.Immutable;
using System.Linq;
using Extensions.Fluxor;
using UI.Models;
using static Extensions.Fluxor.Selectors;

public static class ChooserSelectors
{
    public static readonly ISelector<ChooserState, IImmutableList<GameInfo>> SelectGames = CreateSelector(
        (ChooserState state) => state.Games);

    public static readonly ISelector<ChooserState, int> SelectGameCount = CreateSelector(
        SelectGames, games => games.Count);

    public static readonly ISelector<ChooserState, int> SelectUniqueThemesCount = CreateSelector(
        SelectGames, games => games.Where(g => g.Enabled).Select(o => o.ThemeId).Distinct().Count());

    public static readonly ISelector<ChooserState, bool> SelectAllowGameInCharge = CreateSelector(
       (ChooserState state) => state.AllowGameInCharge);

    public static readonly ISelector<ChooserState, int> SelectDenomFilter = CreateSelector(
        (ChooserState state) => state.DenomFilter);

    public static readonly ISelector<ChooserState, bool> SelectIsSingleGameMode = CreateSelector(
        SelectUniqueThemesCount, SelectAllowGameInCharge, (themes, allow) => themes <= 1 && allow);

    public static readonly ISelector<ChooserState, bool> SelectIsTabView = CreateSelector(
        (ChooserState s) => s.IsTabView);

    public static readonly ISelector<ChooserState, bool> SelectUseSmallIcons = CreateSelector(
        SelectGameCount, SelectIsTabView, (gameCount, isTabView) => isTabView && gameCount > 8);

    public static readonly ISelector<ChooserState, double> SelectChooseGameOffsetY = CreateSelector(
        SelectUseSmallIcons, useSmallIcons => useSmallIcons ? 25.0 : 50.0);
}
