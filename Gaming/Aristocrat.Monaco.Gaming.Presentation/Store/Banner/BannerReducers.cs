namespace Aristocrat.Monaco.Gaming.Presentation.Store.Banner;

using Fluxor;

public static class BannerReducers
{
    [ReducerMethod]
    public static BannerState Reduce(BannerState state, BannerUpdateIdleTextAction action) =>
        state with
        {
            IdleText = action.IdleText
        };

    [ReducerMethod]
    public static BannerState Reduce(BannerState state, BannerUpdateIsScrollingAction action) =>
        state with
        {
            IsScrolling = action.IsScrolling
        };

    [ReducerMethod(typeof(BannerIdleTextTimerAction))]
    public static BannerState Reduce(BannerState state) =>
        state with
        {
            IsIdleTextShowing = true
        };

}