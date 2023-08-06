namespace Aristocrat.Monaco.Gaming.Lobby.Store.Audio;

using System.Threading.Tasks;
using Fluxor;
using Hardware.Contracts.Audio;
using Services.Audio;

internal class AudioEffects
{
    private readonly IState<AudioState> _audioState;
    private readonly IAudioService _audioService;

    public AudioEffects(IState<AudioState> audioState, IAudioService audioService)
    {
        _audioState = audioState;
        _audioService = audioService;
    }

    [EffectMethod]
    public async Task Effect(StartupAction _, IDispatcher dispatcher)
    {
        var playerVolumeScalar = _audioService.GetPlayerVolumeScalar();

        await dispatcher.DispatchAsync(new UpdatePlayerVolumeScalarAction(playerVolumeScalar));

        var soundFiles = _audioService.GetSoundFiles();

        await dispatcher.DispatchAsync(new LoadSoundFilesAction(soundFiles));
    }

    [EffectMethod(typeof(ChangeVolumeAction))]
    public async Task Effect(IDispatcher dispatcher)
    {
        var playerVolumeScalar = _audioState.Value.PlayerVolumeScalar;

        playerVolumeScalar = playerVolumeScalar == VolumeScalar.Scale100 ? VolumeScalar.Scale20 : playerVolumeScalar + 1;

        await dispatcher.DispatchAsync(new UpdatePlayerVolumeScalarAction(playerVolumeScalar));

        _audioService.SetVolume(playerVolumeScalar);

        var sound = _audioState.Value.CurrentSound;

        if (!string.IsNullOrWhiteSpace(sound?.Path))
        {
            await _audioService.PlaySoundAsync(sound);
        }
    }
}
