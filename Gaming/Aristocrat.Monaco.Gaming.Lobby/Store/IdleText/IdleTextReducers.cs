namespace Aristocrat.Monaco.Gaming.Lobby.Store.IdleText;

using Fluxor;

public static class IdleTextReducers
{
    [ReducerMethod]
    public static IdleTextState Reduce(IdleTextState state, UpdateIdleTextAction payload) =>
        state with
        {
            IdleText = payload.IdleText,
        };

    [ReducerMethod]
    public static IdleTextState Reduce(IdleTextState state, UpdateBannerDisplayModeAction payload) =>
        state with
        {
            BannerDisplayMode = payload.Mode,
        };
}
