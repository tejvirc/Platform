namespace Aristocrat.Monaco.Gaming.Presentation.Services.Audio;

using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Audio;
using Hardware.Contracts.Audio;

public interface IAudioService
{
    IEnumerable<SoundFile> GetSoundFiles();

    VolumeScalar GetPlayerVolumeScalar();

    void SetVolume(VolumeScalar volume);

    Task PlaySoundAsync(SoundType sound);

    Task StopSoundAsync(SoundType sound);

    Task PlayGameWinHandPaySound();

    Task PlayLoopingAlert(SoundType sound, int loopCount);
}