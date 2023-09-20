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
    /// <returns>State with updated idle text fields, depending on the source</returns>
    [ReducerMethod]
    public static BannerState Reduce(BannerState state, BannerUpdateIdleTextAction action) =>
        state with
        {
            IdleTextFromCabinetOrHost = (action.TextType is IdleTextType.CabinetOrHost) ? action.IdleText : state.IdleTextFromCabinetOrHost,
            IdleTextFromJurisdiction = (action.TextType is IdleTextType.Jurisdiction) ? action.IdleText : state.IdleTextFromJurisdiction,
            IdleTextDefault = (action.TextType is IdleTextType.Default) ? action.IdleText : state.IdleTextDefault
        };

    /// <summary>
    ///     Handles updates to the IsScrolling state
    /// </summary>
    /// <param name="state"></param>
    /// <param name="action"></param>
    /// <returns>State with updated IsScrolling status</returns>
    [ReducerMethod]
    public static BannerState Reduce(BannerState state, BannerUpdateIsScrollingAction action) =>
        state with
        {
            IsScrolling = action.IsScrolling
        };
}