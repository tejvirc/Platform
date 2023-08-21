namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using Hardware.Contracts.Audio;

public record AudioUpdatePlayerVolumeScalarAction
{
    public AudioUpdatePlayerVolumeScalarAction(VolumeScalar volumeScalar)
    {
        VolumeScalar = volumeScalar;
    }

    public VolumeScalar VolumeScalar { get; }
}
