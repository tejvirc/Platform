namespace Aristocrat.Monaco.Gaming.Lobby.Store.Kernel;

using global::Fluxor;

public static class KernelReducer
{
    [ReducerMethod]
    public static KernelState Reduce(KernelState state, SystemEnabledAction payload) =>
        state with
        {
            IsSystemDisabled = payload.IsDisabled,
            IsSystemDisableImmediately = payload.IsDisableImmediately
        };

    [ReducerMethod]
    public static KernelState Reduce(KernelState state, SystemDisabledAction payload) =>
        state with
        {
            IsSystemDisabled = payload.IsDisabled,
            IsSystemDisableImmediately = payload.IsDisableImmediately
        };
}
