namespace Aristocrat.Monaco.Gaming.Lobby.Store.Application;

using Fluxor;

public static class Reducers
{
    [ReducerMethod]
    public static ApplicationState Reduce(ApplicationState state, SystemEnabledAction payload) =>
        state with
        {
            IsSystemDisabled = payload.IsDisabled,
            IsSystemDisableImmediately = payload.IsDisableImmediately
        };

    [ReducerMethod]
    public static ApplicationState Reduce(ApplicationState state, SystemDisabledAction payload) =>
        state with
        {
            IsSystemDisabled = payload.IsDisabled,
            IsSystemDisableImmediately = payload.IsDisableImmediately
        };
}
