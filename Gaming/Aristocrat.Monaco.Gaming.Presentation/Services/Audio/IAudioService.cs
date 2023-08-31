namespace Aristocrat.Monaco.Gaming.Presentation.Services.Audio;

using System.Collections.Generic;
using System.Threading.Tasks;
using Hardware.Contracts.Audio;
using Models;

public interface IAudioService
{
    IEnumerable<SoundFile> GetSoundFiles();

    VolumeScalar GetPlayerVolumeScalar();

    void SetVolume(VolumeScalar volume);

    Task PlaySoundAsync(SoundType sound);
}
