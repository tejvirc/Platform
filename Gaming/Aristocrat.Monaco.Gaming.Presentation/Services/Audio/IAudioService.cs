namespace Aristocrat.Monaco.Gaming.Presentation.Services.Audio;

using System.Collections.Generic;
using System.Threading.Tasks;
using Application.UI.ViewModels;
using Hardware.Contracts.Audio;

public interface IAudioService
{
    IEnumerable<SoundFileViewModel> GetSoundFiles();

    VolumeScalar GetPlayerVolumeScalar();

    void SetVolume(VolumeScalar volume);

    Task PlaySoundAsync(SoundFileViewModel sound);
}
