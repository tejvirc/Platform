namespace Aristocrat.Monaco.Gaming.Presentation.Store.Audio;

using System.Collections.Immutable;
using Hardware.Contracts.Audio;
using Models;

public record AudioState
{
    public IImmutableList<SoundFile> SoundFiles { get; init; }

    public VolumeScalar PlayerVolumeScalar { get; init; }
}
