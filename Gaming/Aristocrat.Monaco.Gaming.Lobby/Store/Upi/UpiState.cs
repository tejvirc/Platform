namespace Aristocrat.Monaco.Gaming.Lobby.Store.Upi;

public record UpiState
{
    public bool IsServiceAvailable { get; init; }

    public bool IsServiceEnabled { get; init; }

    public bool IsVolumeControlEnabled { get; init; }
}
