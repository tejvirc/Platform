namespace Aristocrat.Monaco.Gaming.Presentation.Contracts.Audio;

public class SoundFile
{
    public SoundFile(SoundType sound, string path)
    {
        Sound = sound;
        Path = path;
    }

    public SoundType Sound { get; set; }

    public string Path { get; set; }
}
