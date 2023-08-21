namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attendant;

public record AttendantState
{
    public bool IsServiceAvailable { get; init; }

    public bool IsServiceEnabled { get; init; }
}
