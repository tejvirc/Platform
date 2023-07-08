namespace Aristocrat.Monaco.Gaming.Lobby.Store.Common;

using Fluxor;

public static class CommonReducers
{
    [ReducerMethod]
    public static CommonState Reduce(CommonState state, SystemEnabledAction payload) =>
        state with
        {
            IsSystemDisabled = payload.IsDisabled,
            IsSystemDisableImmediately = payload.IsDisableImmediately
        };

    [ReducerMethod]
    public static CommonState Reduce(CommonState state, SystemDisabledAction payload) =>
        state with
        {
            IsSystemDisabled = payload.IsDisabled,
            IsSystemDisableImmediately = payload.IsDisableImmediately
        };
}
