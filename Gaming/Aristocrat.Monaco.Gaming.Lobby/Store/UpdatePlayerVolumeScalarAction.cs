namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using Hardware.Contracts.Audio;

public record UpdatePlayerVolumeScalarAction
{
    public UpdatePlayerVolumeScalarAction(VolumeScalar volumeScalar)
    {
        VolumeScalar = volumeScalar;
    }

    public VolumeScalar VolumeScalar { get; }
}
