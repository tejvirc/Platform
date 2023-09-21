namespace Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;

using Fluxor;

public static class InfoBarReducers
{
    [ReducerMethod()]
    public static InfoBarState Reduce(InfoBarState state, InfoBarRequestOpenAction action)
    {
        return state with
        {
            MainInfoBarOpenRequested = action.MainInfoBarOpenRequested,
            VbdInfoBarOpenRequested = action.VbdInfoBarOpenRequested
        };
    }

    [ReducerMethod()]
    public static InfoBarState Reduce(InfoBarState state, InfoBarCloseAction action)
    {
        return state with
        {
            IsOpen = action.IsOpen,
        };
    }

    [ReducerMethod()]
    public static InfoBarState Reduce(InfoBarState state, InfoBarLeftRegionUpdateAction action)
    {
        return state with
        {
            LeftRegionText = action.Text,
            LeftRegionTextColor = action.TextColor,
            LeftRegionDuration = action.Duration
        };
    }

    [ReducerMethod()]
    public static InfoBarState Reduce(InfoBarState state, InfoBarCenterRegionUpdateAction action)
    {
        return state with
        {
            CenterRegionText = action.Text,
            CenterRegionTextColor = action.TextColor,
            CenterRegionDuration = action.Duration
        };
    }

    [ReducerMethod()]
    public static InfoBarState Reduce(InfoBarState state, InfoBarRightRegionUpdateAction action)
    {
        return state with
        {
            RightRegionText = action.Text,
            RightRegionTextColor = action.TextColor,
            RightRegionDuration = action.Duration
        };
    }
}
