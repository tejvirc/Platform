namespace Aristocrat.Monaco.Gaming.Presentation.Store.Platform;

using Fluxor;

public static class PlatformReducers
{
    [ReducerMethod]
    public static PlatformState PlatformEnabled(PlatformState state, PlatformEnabledAction action) =>
        state with
        {
            IsDisabled = action.IsDisabled,
            IsDisableImmediately = action.IsDisableImmediately
        };

    [ReducerMethod]
    public static PlatformState PlatformDisabled(PlatformState state, PlatformDisabledAction action) =>
        state with
        {
            IsDisabled = action.IsDisabled,
            IsDisableImmediately = action.IsDisableImmediately
        };

    [ReducerMethod(typeof(PlatformDisplayConnectedAction))]
    public static PlatformState DisplayConnected(PlatformState state) =>
        state with
        {
            IsDisplayConnected = true
        };

    [ReducerMethod(typeof(PlatformDisplayDisconnectedAction))]
    public static PlatformState DisplayDisconnected(PlatformState state) =>
        state with
        {
            IsDisplayConnected = false
        };
}
