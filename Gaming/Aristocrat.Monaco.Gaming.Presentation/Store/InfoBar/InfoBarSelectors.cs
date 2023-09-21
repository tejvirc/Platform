namespace Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;

using Cabinet.Contracts;
using Extensions.Fluxor;
using Gaming.Contracts.InfoBar;
using static Extensions.Fluxor.Selectors;

public static class InfoBarSelectors
{
    public static readonly ISelector<InfoBarState, bool> SelectMainInfoBarOpenRequested = CreateSelector(
        (InfoBarState state) => state.MainInfoBarOpenRequested);

    public static readonly ISelector<InfoBarState, bool> SelectVbdInfoBarOpenRequested = CreateSelector(
        (InfoBarState state) => state.VbdInfoBarOpenRequested);

    public static readonly ISelector<InfoBarState, bool> SelectInfoBarIsOpen = CreateSelector(
        (InfoBarState state) => state.IsOpen);

    public static readonly ISelector<InfoBarState, string?> SelectInfoBarLeftRegionText = CreateSelector(
        (InfoBarState state) => state.LeftRegionText);

    public static readonly ISelector<InfoBarState, string?> SelectInfoBarCenterRegionText = CreateSelector(
        (InfoBarState state) => state.CenterRegionText);

    public static readonly ISelector<InfoBarState, string?> SelectInfoBarRightRegionText = CreateSelector(
        (InfoBarState state) => state.RightRegionText);

    public static readonly ISelector<InfoBarState, double> SelectInfoBarLeftRegionDuration = CreateSelector(
        (InfoBarState state) => state.LeftRegionDuration);

    public static readonly ISelector<InfoBarState, double> SelectInfoBarCenterRegionDuration = CreateSelector(
        (InfoBarState state) => state.CenterRegionDuration);

    public static readonly ISelector<InfoBarState, double> SelectInfoBarRightRegionDuration = CreateSelector(
        (InfoBarState state) => state.RightRegionDuration);

    public static readonly ISelector<InfoBarState, InfoBarColor> SelectInfoBarLeftRegionTextColor = CreateSelector(
        (InfoBarState state) => state.LeftRegionTextColor);

    public static readonly ISelector<InfoBarState, InfoBarColor> SelectInfoBarCenterRegionTextColor = CreateSelector(
        (InfoBarState state) => state.CenterRegionTextColor);

    public static readonly ISelector<InfoBarState, InfoBarColor> SelectInfoBarRightRegionTextColor = CreateSelector(
        (InfoBarState state) => state.RightRegionTextColor);

    public static readonly ISelector<InfoBarState, DisplayRole> SelectInfoBarDisplayTarget = CreateSelector(
        (InfoBarState state) => state.DisplayTarget);
}
