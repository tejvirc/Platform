namespace Aristocrat.Monaco.Gaming.Presentation.Store.Audio;

using System.Collections.Immutable;
using Contracts.Audio;
using Hardware.Contracts.Audio;

public record AudioState
{
    public IImmutableList<SoundFile> SoundFiles { get; init; }

    public VolumeScalar PlayerVolumeScalar { get; init; }
}
