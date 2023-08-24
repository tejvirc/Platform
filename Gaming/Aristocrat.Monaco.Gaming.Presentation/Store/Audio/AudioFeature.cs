namespace Aristocrat.Monaco.Gaming.Presentation.Store.Audio;

using System.Collections.Immutable;
using Fluxor;
using Models;

public class AudioFeature : Feature<AudioState>
{
    public override string GetName() => "Audio";

    protected override AudioState GetInitialState()
    {
        return new AudioState
        {
            SoundFiles = ImmutableList<SoundFile>.Empty
        };
    }
}
