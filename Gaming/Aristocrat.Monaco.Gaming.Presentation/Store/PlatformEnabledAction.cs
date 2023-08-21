namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record PlatformEnabledAction
{
    public PlatformEnabledAction(
        bool isDisabled,
        bool isDisableImmediately)
    {
        IsDisabled = isDisabled;
        IsDisableImmediately = isDisableImmediately;
    }

    public bool IsDisabled { get; }

    public bool IsDisableImmediately { get; }
}
