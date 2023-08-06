namespace Aristocrat.Monaco.Gaming.Lobby.Store.Audio;

using System.Collections.Immutable;
using Application.UI.ViewModels;
using Hardware.Contracts.Audio;

public record AudioState
{
    public IImmutableList<SoundFileViewModel> SoundFiles { get; init; }

    public VolumeScalar PlayerVolumeScalar { get; init; }

    public SoundFileViewModel? CurrentSound { get; init; }
}
