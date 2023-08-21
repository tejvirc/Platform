namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attendant;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class AttendantSelectors
{
    public static readonly ISelector<AttendantState, bool> SelectServiceAvailable = CreateSelector(
        (AttendantState state) => state.IsServiceAvailable);

    public static readonly ISelector<AttendantState, bool> SelectServiceEnabled = CreateSelector(
        (AttendantState state) => state.IsServiceEnabled);
}
