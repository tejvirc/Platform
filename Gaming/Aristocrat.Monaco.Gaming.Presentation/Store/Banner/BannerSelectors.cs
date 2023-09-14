namespace Aristocrat.Monaco.Gaming.Presentation.Store.Banner;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class BannerSelectors
{
    public static readonly ISelector<BannerState, string?> IdleTextSelector = CreateSelector(
        (BannerState s) => s.IdleText);
    public static readonly ISelector<BannerState, bool> IsIdleTextShowingSelector = CreateSelector(
        (BannerState s) => s.IsIdleTextShowing);
    public static readonly ISelector<BannerState, bool> IsPausedSelector = CreateSelector(
        (BannerState s) => s.IsPaused);
    public static readonly ISelector<BannerState, bool> IsScrollingSelector = CreateSelector(
        (BannerState s) => s.IsScrolling);
}