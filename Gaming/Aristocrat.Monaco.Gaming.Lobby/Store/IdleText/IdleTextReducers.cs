namespace Aristocrat.Monaco.Gaming.Lobby.Store.IdleText;

using Fluxor;

public static class IdleTextReducers
{
    [ReducerMethod]
    public static IdleTextState Reduce(IdleTextState state, UpdateIdleTextAction action) =>
        state with
        {
            IdleText = action.IdleText,
        };

    [ReducerMethod]
    public static IdleTextState Reduce(IdleTextState state, UpdateBannerDisplayModeAction action) =>
        state with
        {
            BannerDisplayMode = action.Mode,
        };
}
