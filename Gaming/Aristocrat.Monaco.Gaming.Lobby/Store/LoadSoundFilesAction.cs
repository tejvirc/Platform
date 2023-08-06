namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System.Collections.Generic;
using System.Collections.Immutable;
using Application.UI.ViewModels;

public record LoadSoundFilesAction
{
    public LoadSoundFilesAction(IEnumerable<SoundFileViewModel> soundFiles)
    {
        SoundFiles = ImmutableList.CreateRange(soundFiles);
    }

    public ImmutableList<SoundFileViewModel> SoundFiles { get; }
}
