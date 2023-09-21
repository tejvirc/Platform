namespace Aristocrat.Monaco.Gaming.Presentation.Store.Banner;

using Fluxor;

/// <summary>
///     Reducers for the Banner component
/// </summary>
public static class BannerReducers
{
    /// <summary>
    ///     Handles updates to idle text
    /// </summary>
    /// <param name="state"></param>
    /// <param name="action"></param>
    /// <returns>State with updated idle text to display</returns>
    [ReducerMethod]
    public static BannerState Reduce(BannerState state, BannerUpdateIdleTextAction action)
    {
        return state with { CurrentIdleText = action.IdleText };
    }

    /// <summary>
    ///     Handles updates to the IsScrolling state
    /// </summary>
    /// <param name="state"></param>
    /// <param name="action"></param>
    /// <returns>State with updated IsScrolling status</returns>
    [ReducerMethod]
    public static BannerState Reduce(BannerState state, BannerUpdateIsScrollingAction action)
    {
        return state with { IsScrolling = action.IsScrolling };
    }
}