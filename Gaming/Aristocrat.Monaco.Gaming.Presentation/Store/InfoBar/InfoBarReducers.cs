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
}
