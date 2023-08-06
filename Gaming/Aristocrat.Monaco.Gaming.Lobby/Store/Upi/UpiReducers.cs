namespace Aristocrat.Monaco.Gaming.Lobby.Store.Upi;

using Fluxor;

public static class UpiReducers
{
    [ReducerMethod]
    public static UpiState Reduce(UpiState state, UpdateServiceAvailable action) =>
        state with
        {
            IsServiceAvailable = action.IsAvaiable,
            IsServiceEnabled = action.IsAvaiable
        };

    [ReducerMethod]
    public static UpiState Reduce(UpiState state, UpdateVolumeControlEnabled action) =>
        state with
        {
            IsVolumeControlEnabled = action.IsEnabled
        };
}
