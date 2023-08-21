namespace Aristocrat.Monaco.Gaming.Presentation.Store.Upi;

using Fluxor;

public static class UpiReducers
{
    [ReducerMethod]
    public static UpiState Reduce(UpiState state, AttendantUpdateServiceAvailableAction action) =>
        state with
        {
            IsServiceAvailable = action.IsAvaiable,
            IsServiceEnabled = action.IsAvaiable
        };

    [ReducerMethod]
    public static UpiState Reduce(UpiState state, AudioUpdateVolumeControlEnabledAction action) =>
        state with
        {
            IsVolumeControlEnabled = action.IsEnabled
        };
}
