namespace Aristocrat.Monaco.Gaming.Presentation.Store.Upi;

public record UpiState
{
    public bool IsServiceAvailable { get; init; }

    public bool IsServiceEnabled { get; init; }

    public bool IsVolumeControlEnabled { get; init; }
}
