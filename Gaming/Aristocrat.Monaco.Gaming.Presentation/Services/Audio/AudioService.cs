namespace Aristocrat.Monaco.Gaming.Presentation.Services.Audio;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts;
using Fluxor;
using Hardware.Contracts.Audio;
using Kernel;
using Microsoft.Extensions.Logging;
using Models;
using Store.Audio;

public class AudioService : IAudioService
{
    private const string SoundConfigurationExtensionPath = "/OperatorMenu/Sound/Configuration";
    private const double PaperInChuteAlertVolumeRate = 0.8;
    private const byte DefaultAlertVolume = 100;

    private readonly ILogger<AudioService> _logger;
    private readonly IState<AudioState> _audioState;
    private readonly IPropertiesManager _properties;
    private readonly IAudio _audio;

    public AudioService(
        ILogger<AudioService> logger,
        IState<AudioState> audioState,
        IPropertiesManager properties,
        IAudio audio)
    {
        _logger = logger;
        _audioState = audioState;
        _properties = properties;
        _audio = audio;
    }

    public IEnumerable<SoundFile> GetSoundFiles()
    {
        var files = new Dictionary<SoundType, string?>
        {
            {
                SoundType.First, MonoAddinsHelper
                    .GetSelectedNodes<FilePathExtensionNode>(SoundConfigurationExtensionPath)
                    .Take(1)
                    .Select(node => node.FilePath)
                    .First()
            },
            { SoundType.Touch, _properties.GetValue(ApplicationConstants.TouchSoundKey, string.Empty) },
            { SoundType.CoinIn, _properties.GetValue(ApplicationConstants.CoinInSoundKey, string.Empty) },
            { SoundType.CoinOut, _properties.GetValue(ApplicationConstants.CoinOutSoundKey, string.Empty) },
            { SoundType.FeatureBell, _properties.GetValue(ApplicationConstants.FeatureBellSoundKey, string.Empty) },
            { SoundType.Collect, _properties.GetValue(ApplicationConstants.CollectSoundKey, string.Empty) },
            { SoundType.PaperInChute, _properties.GetValue(ApplicationConstants.PaperInChuteSoundKey, string.Empty) }
        };

        var sounds = files
            .Where(x => !string.IsNullOrEmpty(x.Value))
            .Select(x => new SoundFile(x.Key, x.Value!));

        foreach (var sound in sounds)
        {
            _audio.Load(Path.GetFullPath(sound.Path));
        }

        return sounds;
    }

    public VolumeScalar GetPlayerVolumeScalar()
    {
        return _properties.GetValue(
            ApplicationConstants.PlayerVolumeScalarKey,
            (VolumeScalar)ApplicationConstants.PlayerVolumeScalar);
    }

    public void SetVolume(VolumeScalar volume)
    {
        _properties.SetProperty(ApplicationConstants.PlayerVolumeScalarKey, volume);
    }

    public Task PlaySoundAsync(SoundType sound)
    {
        var tcs = new TaskCompletionSource<bool>();

        var soundFile = _audioState.Value.SoundFiles.FirstOrDefault(x => x.Sound == sound) ??
                        throw new ArgumentException("Sound not found", nameof(sound));

        var volume = GetVolume();

        _audio.Play(soundFile.Path, volume, callback: OnCompleted);

        return tcs.Task;

        void OnCompleted()
        {
            tcs.SetResult(true);
        }
    }

    public Task StopSoundAsync(SoundType sound)
    {
        var soundFile = _audioState.Value.SoundFiles.FirstOrDefault(x => x.Sound == sound) ??
                        throw new ArgumentException("Sound not found", nameof(sound));

        _audio.Stop(soundFile.Path);

        return Task.CompletedTask;
    }

    public Task PlayGameWinHandPaySound()
    {
        var tcs = new TaskCompletionSource<bool>();

        var featureBellSound = _audioState.Value.SoundFiles.FirstOrDefault(x => x.Sound == SoundType.FeatureBell) ??
                               throw new ArgumentException("Sound not found", nameof(SoundType.FeatureBell));

        var collectSound = _audioState.Value.SoundFiles.FirstOrDefault(x => x.Sound == SoundType.Collect) ??
                           throw new ArgumentException("Sound not found", nameof(SoundType.Collect));

        var volume = GetVolume();

        _audio.Play(featureBellSound.Path, volume, callback: OnCompletedFeatureBell);

        return tcs.Task;

        void OnCompletedFeatureBell()
        {
            _audio.Play(collectSound.Path, volume, callback: OnCompleted);
        }

        void OnCompleted()
        {
            tcs.SetResult(true);
        }
    }

    public Task PlayLoopingAlert(SoundType sound, int loopCount)
    {
        return PlayLoopingSoundAsync(
            sound,
            loopCount,
            _properties.GetValue(ApplicationConstants.AlertVolumeKey, DefaultAlertVolume));
    }

    private Task PlayLoopingSoundAsync(SoundType sound, int loopCount, float volume)
    {
        var tcs = new TaskCompletionSource<bool>();

        var soundFile = _audioState.Value.SoundFiles.FirstOrDefault(x => x.Sound == sound) ??
                        throw new ArgumentException("Sound not found", nameof(sound));

        if (sound == SoundType.PaperInChute && _audio.IsPlaying(soundFile.Path))
        {
            tcs.SetResult(true);
        }

        volume = (float)(volume * PaperInChuteAlertVolumeRate);

        _audio.Play(soundFile.Path, loopCount, volume, callback: OnCompleted);

        return tcs.Task;

        void OnCompleted()
        {
            tcs.SetResult(true);
        }
    }

    private float GetVolume()
    {
        var volumeScalar = (VolumeScalar)_properties.GetValue(
            ApplicationConstants.LobbyVolumeScalarKey,
            ApplicationConstants.LobbyVolumeScalar);

        return _audio.GetDefaultVolume() *
               _audio.GetVolumeScalar(_audioState.Value.PlayerVolumeScalar) *
               _audio.GetVolumeScalar(volumeScalar);
    }
}