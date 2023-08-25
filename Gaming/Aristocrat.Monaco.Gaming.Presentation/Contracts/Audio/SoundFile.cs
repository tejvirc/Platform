namespace Aristocrat.Monaco.Gaming.Presentation.Contracts.Audio;

public class SoundFile
{
    private SoundType _sound;
    private string _path;

    public SoundFile(SoundType sound, string path)
    {
        _sound = sound;
        _path = path;
    }

    public SoundType Sound
    {
        get => _sound;

        set => SetProperty(ref _sound, value);
    }

    public string Path
    {
        get => _path;

        set => SetProperty(ref _path, value);
    }
}
