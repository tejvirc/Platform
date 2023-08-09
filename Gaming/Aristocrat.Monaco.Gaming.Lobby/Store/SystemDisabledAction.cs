namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using Kernel;

public record SystemDisabledAction
{
    public SystemDisabledAction(
        SystemDisablePriority priority,
        bool isSystemDisabled,
        bool isSystemDisableImmediately)
    {
        Priority = priority;
        IsSystemDisabled = isSystemDisabled;
        IsSystemDisableImmediately = isSystemDisableImmediately;
    }

    public SystemDisablePriority Priority { get; }

    public bool IsSystemDisabled { get; }

    public bool IsSystemDisableImmediately { get; }
}
