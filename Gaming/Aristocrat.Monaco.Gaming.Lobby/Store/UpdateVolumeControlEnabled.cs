namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record UpdateVolumeControlEnabled
{
    public UpdateVolumeControlEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public bool IsEnabled { get; }
}
