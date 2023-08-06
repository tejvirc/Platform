namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attendant;

using Fluxor;

public record AttendantReducer
{
    [ReducerMethod]
    public static AttendantState Reduce(AttendantState state, UpdateServiceAvailable action) =>
        state with
        {
            IsServiceAvailable = action.IsAvaiable,
            IsServiceEnabled = action.IsAvaiable
        };
}
