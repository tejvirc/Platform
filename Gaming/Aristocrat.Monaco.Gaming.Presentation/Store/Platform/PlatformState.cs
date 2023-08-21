namespace Aristocrat.Monaco.Gaming.Presentation.Store.Platform;

public record PlatformState
{
    public bool IsDisabled { get; init; }

    public bool IsDisableImmediately { get; init; }

    public bool IsDisplayConnected { get; init; }
}
