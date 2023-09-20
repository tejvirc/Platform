namespace Aristocrat.Monaco.Gaming.Presentation.Store.Banner;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class BannerSelectors
{
    /// <summary>
    ///     Gets the idle text to show, based on precedence (host/cabinet-provided, else jurisdiction-override, else default)
    /// </summary>
    public static readonly ISelector<BannerState, string?> IdleTextSelector = CreateSelector(
        (BannerState s) => s.IdleTextFromCabinetOrHost ?? s.IdleTextFromJurisdiction ?? s.IdleTextDefault);

    /// <summary>
    ///     Gets whether or not the current banner display is paused due to platform being disabled
    /// </summary>
    public static readonly ISelector<BannerState, bool> IsPausedSelector = CreateSelector(
        (BannerState s) => s.IsPaused);
    /// <summary>
    ///     Gets whether or not the idle text is in scrolling mode and actively scrolling still (in case need to wait for finish)
    /// </summary>
    public static readonly ISelector<BannerState, bool> IsScrollingSelector = CreateSelector(
        (BannerState s) => s.IsScrolling);
}