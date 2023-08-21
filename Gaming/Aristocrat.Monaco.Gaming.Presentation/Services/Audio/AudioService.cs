namespace Aristocrat.Monaco.Gaming.Presentation.Services.Audio;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Application.Contracts;
using Application.UI.ViewModels;
using Fluxor;
using Hardware.Contracts.Audio;
using Kernel;
using Microsoft.Extensions.Logging;
using Store.Audio;

public class AudioService : IAudioService
{
    private const string SoundConfigurationExtensionPath = "/OperatorMenu/Sound/Configuration";
    private readonly ILogger<AudioService> _logger;
    private readonly IState<AudioState> _audioState;
    private readonly IPropertiesManager _properties;
    private readonly IAudio _audio;

    public AudioService(ILogger<AudioService> logger, IState<AudioState> audioState, IPropertiesManager properties, IAudio audio)
    {
        _logger = logger;
        _audioState = audioState;
        _properties = properties;
        _audio = audio;
    }

    public IEnumerable<SoundFileViewModel> GetSoundFiles()
    {
        var files = new List<SoundFileViewModel>();

        var nodes =
            MonoAddinsHelper.GetSelectedNodes<FilePathExtensionNode>(
                SoundConfigurationExtensionPath);

        foreach (var node in nodes)
        {
            var path = node.FilePath;
            var name = !string.IsNullOrWhiteSpace(node.Name)
                ? node.Name
                : Path.GetFileNameWithoutExtension(path);

            _logger.LogDebug(
                "Found {SoundConfigurationExtensionPath} node: {SoundFilePath}", SoundConfigurationExtensionPath, node.FilePath);

            files.Add(new SoundFileViewModel(name, path));
        }

        return files;
    }

    public VolumeScalar GetPlayerVolumeScalar() =>
        _properties.GetValue(
                ApplicationConstants.PlayerVolumeScalarKey,
                (VolumeScalar)ApplicationConstants.PlayerVolumeScalar);

    public void SetVolume(VolumeScalar volume)
    {
        _properties.SetProperty(ApplicationConstants.PlayerVolumeScalarKey, volume);
    }

    public Task PlaySoundAsync(SoundFileViewModel sound)
    {
        var tcs = new TaskCompletionSource<bool>();

        if (string.IsNullOrWhiteSpace(sound?.Path))
        {
            throw new ArgumentNullException(nameof(sound), "Either object is null or path is null or empty");
        }

        var volume = GetVolume();

        _audio.Play(sound.Path, volume, callback: OnCompleted);

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

        return _audio.GetDefaultVolume() * _audio.GetVolumeScalar(_audioState.Value.PlayerVolumeScalar) *
                    _audio.GetVolumeScalar(volumeScalar);
    }
}
