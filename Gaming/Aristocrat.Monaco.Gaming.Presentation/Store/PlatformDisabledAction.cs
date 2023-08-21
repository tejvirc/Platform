namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using Kernel;

public record PlatformDisabledAction
{
    public PlatformDisabledAction(
        SystemDisablePriority priority,
        bool isDisabled,
        bool isDisableImmediately)
    {
        Priority = priority;
        IsDisabled = isDisabled;
        IsDisableImmediately = isDisableImmediately;
    }

    public SystemDisablePriority Priority { get; }

    public bool IsDisabled { get; }

    public bool IsDisableImmediately { get; }
}
