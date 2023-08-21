namespace Aristocrat.Monaco.Gaming.Presentation.Store.IdleText;

using Fluxor;

public static class IdleTextReducers
{
    [ReducerMethod]
    public static IdleTextState Reduce(IdleTextState state, IdleTextUpdateTextAction action) =>
        state with
        {
            IdleText = action.IdleText,
        };

    [ReducerMethod]
    public static IdleTextState Reduce(IdleTextState state, BannerUpdateDisplayModeAction action) =>
        state with
        {
            BannerDisplayMode = action.Mode,
        };
}
