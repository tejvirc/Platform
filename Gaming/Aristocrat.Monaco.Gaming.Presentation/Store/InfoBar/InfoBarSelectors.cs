namespace Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class InfoBarSelectors
{
    public static readonly ISelector<InfoBarState, bool> SelectMainInfoBarOpenRequested = CreateSelector(
        (InfoBarState state) => state.MainInfoBarOpenRequested);

    public static readonly ISelector<InfoBarState, bool> SelectVbdInfoBarOpenRequested = CreateSelector(
        (InfoBarState state) => state.VbdInfoBarOpenRequested);

    public static readonly ISelector<InfoBarState, bool> SelectIsOpen = CreateSelector(
        (InfoBarState state) => state.IsOpen);
}
