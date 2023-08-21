namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attendant;

using Fluxor;

public record AttendantReducer
{
    [ReducerMethod]
    public static AttendantState UpdateServiceAvailable(AttendantState state, AttendantUpdateServiceAvailableAction action) =>
        state with
        {
            IsServiceAvailable = action.IsAvaiable,
            IsServiceEnabled = action.IsAvaiable
        };
}
