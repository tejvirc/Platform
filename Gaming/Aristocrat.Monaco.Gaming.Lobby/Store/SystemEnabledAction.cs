namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record SystemEnabledAction
{
    public SystemEnabledAction(
        bool isSystemDisabled,
        bool isSystemDisableImmediately)
    {
        IsSystemDisabled = isSystemDisabled;
        IsSystemDisableImmediately = isSystemDisableImmediately;
    }

    public bool IsSystemDisabled { get; }

    public bool IsSystemDisableImmediately { get; }
}
