namespace Aristocrat.Monaco.Gaming.Lobby.Store.Audio;

using System.Collections.Immutable;
using Application.UI.ViewModels;
using Fluxor;

internal class AudioFeature : Feature<AudioState>
{
    public override string GetName() => "Audio";

    protected override AudioState GetInitialState()
    {
        return new AudioState
        {
            SoundFiles = ImmutableList<SoundFileViewModel>.Empty,
        };
    }
}
