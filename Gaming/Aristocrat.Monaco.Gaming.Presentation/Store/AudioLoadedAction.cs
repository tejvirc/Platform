namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System.Collections.Generic;
using System.Collections.Immutable;
using Models;

public record AudioLoadedAction
{
    public AudioLoadedAction(IEnumerable<SoundFile> soundFiles)
    {
        SoundFiles = ImmutableList.CreateRange(soundFiles);
    }

    public ImmutableList<SoundFile> SoundFiles { get; }
}
