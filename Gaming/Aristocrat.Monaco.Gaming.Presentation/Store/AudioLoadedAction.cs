namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System.Collections.Generic;
using System.Collections.Immutable;
using Application.UI.ViewModels;

public record AudioLoadedAction
{
    public AudioLoadedAction(IEnumerable<SoundFileViewModel> soundFiles)
    {
        SoundFiles = ImmutableList.CreateRange(soundFiles);
    }

    public ImmutableList<SoundFileViewModel> SoundFiles { get; }
}
