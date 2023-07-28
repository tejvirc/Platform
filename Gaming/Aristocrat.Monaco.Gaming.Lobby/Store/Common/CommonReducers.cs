namespace Aristocrat.Monaco.Gaming.Lobby.Store.Common;

using Fluxor;

public static class CommonReducers
{
    [ReducerMethod]
    public static CommonState Reduce(CommonState state, SystemEnabledAction action) =>
        state with
        {
            IsSystemDisabled = action.IsSystemDisabled,
            IsSystemDisableImmediately = action.IsSystemDisableImmediately
        };

    [ReducerMethod]
    public static CommonState Reduce(CommonState state, SystemDisabledAction action) =>
        state with
        {
            IsSystemDisabled = action.IsSystemDisabled,
            IsSystemDisableImmediately = action.IsSystemDisableImmediately
        };
}
