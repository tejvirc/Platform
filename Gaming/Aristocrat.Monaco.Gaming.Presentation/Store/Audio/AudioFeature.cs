namespace Aristocrat.Monaco.Gaming.Presentation.Store.Audio;

using System.Collections.Immutable;
using Contracts.Audio;
using Fluxor;

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
