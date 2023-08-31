namespace Aristocrat.Monaco.Gaming.Presentation.Models;

using CommunityToolkit.Mvvm.ComponentModel;

public class SoundFile : ObservableObject
{
    public SoundFile(SoundType sound, string path)
    {
        Sound = sound;
        Path = path;
    }

    public SoundType Sound { get; set; }

    public string Path { get; set; }
}
