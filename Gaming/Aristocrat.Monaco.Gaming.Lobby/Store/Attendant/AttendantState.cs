namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attendant;

public record AttendantState
{
    public bool IsServiceAvailable { get; init; }

    public bool IsServiceEnabled { get; init; }
}
