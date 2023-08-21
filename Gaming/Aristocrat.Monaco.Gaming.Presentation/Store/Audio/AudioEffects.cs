namespace Aristocrat.Monaco.Gaming.Presentation.Store.Audio;

using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Hardware.Contracts.Audio;
using Services.Audio;

public class AudioEffects
{
    private readonly IState<AudioState> _audioState;
    private readonly IAudioService _audioService;

    public AudioEffects(IState<AudioState> audioState, IAudioService audioService)
    {
        _audioState = audioState;
        _audioService = audioService;
    }

    [EffectMethod(typeof(StartupAction))]
    public async Task Startup(IDispatcher dispatcher)
    {
        var playerVolumeScalar = _audioService.GetPlayerVolumeScalar();

        await dispatcher.DispatchAsync(new AudioUpdatePlayerVolumeScalarAction(playerVolumeScalar));

        var soundFiles = _audioService.GetSoundFiles();

        await dispatcher.DispatchAsync(new AudioLoadedAction(soundFiles));
    }

    [EffectMethod(typeof(AudioChangeVolumeAction))]
    public async Task ChangeVolume(IDispatcher dispatcher)
    {
        var playerVolumeScalar = _audioState.Value.PlayerVolumeScalar;

        playerVolumeScalar = playerVolumeScalar == VolumeScalar.Scale100 ? VolumeScalar.Scale20 : playerVolumeScalar + 1;

        await dispatcher.DispatchAsync(new AudioUpdatePlayerVolumeScalarAction(playerVolumeScalar));

        _audioService.SetVolume(playerVolumeScalar);

        var sound = _audioState.Value.CurrentSound;

        if (!string.IsNullOrWhiteSpace(sound?.Path))
        {
            await _audioService.PlaySoundAsync(sound);
        }
    }
}
