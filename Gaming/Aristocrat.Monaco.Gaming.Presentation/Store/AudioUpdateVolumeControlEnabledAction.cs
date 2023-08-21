namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record AudioUpdateVolumeControlEnabledAction
{
    public AudioUpdateVolumeControlEnabledAction(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public bool IsEnabled { get; }
}
