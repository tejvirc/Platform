namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.UI.ViewModels;
    using Hardware.Contracts.Audio;
    using Kernel;
    using log4net;

    public class LobbyVolumeViewModel : BaseEntityViewModel
    {
        private const string SoundConfigurationExtensionPath = "/OperatorMenu/Sound/Configuration";
        private const string Volume0Key = "Volume0Normal";
        private const string Volume1Key = "Volume1Normal";
        private const string Volume2Key = "Volume2Normal";
        private const string Volume3Key = "Volume3Normal";
        private const string Volume4Key = "Volume4Normal";
        private const string Volume5Key = "Volume5Normal";
        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAudio _audio;
        private readonly Action _onUserInteraction;
        private VolumeScalar _playerVolumeScalar;
        private SoundFileViewModel _soundFile;

        public LobbyVolumeViewModel(Action onUserInteraction)
        {
            _onUserInteraction = onUserInteraction;
            _propertiesManager = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
            _audio = ServiceManager.GetInstance().TryGetService<IAudio>();

            _playerVolumeScalar = (VolumeScalar)_propertiesManager.GetValue(
                ApplicationConstants.PlayerVolumeScalarKey,
                ApplicationConstants.PlayerVolumeScalar);
            Logger.DebugFormat("Initializing default volume setting with value: {0}", PlayerVolumeScalar);
            VolumeCommand = new ActionCommand<object>(o => OnVolumeChange());
            LoadSoundFile();
        }

        public ICommand VolumeCommand { get; }

        public VolumeScalar PlayerVolumeScalar
        {
            get => _playerVolumeScalar;
            set
            {
                if (_playerVolumeScalar != value)
                {
                    _playerVolumeScalar = value;
                    SetVolumeAndPlaySound();
                    RaisePropertyChanged(nameof(PlayerVolumeScalar));
                    RaisePropertyChanged(nameof(VolumeValue));
                }
            }
        }

        public int VolumeValue => (int)PlayerVolumeScalar;

        public List<string> ResourceKeys { get; } = new List<string>
        {
            Volume0Key,
            Volume1Key,
            Volume2Key,
            Volume3Key,
            Volume4Key,
            Volume5Key
        };

        public float GetVolume(IAudio audio)
        {
            var volume = 0.0f;

            if (audio != null)
            {
                var lobbyVolumeScalar = (VolumeScalar)_propertiesManager.GetValue(
                    ApplicationConstants.LobbyVolumeScalarKey,
                    ApplicationConstants.LobbyVolumeScalar);
                volume = audio.GetDefaultVolume() * audio.GetVolumeScalar(PlayerVolumeScalar) *
                         audio.GetVolumeScalar(lobbyVolumeScalar);
            }

            return volume;
        }

        private void OnVolumeChange()
        {
            if (PlayerVolumeScalar == VolumeScalar.Scale100)
            {
                PlayerVolumeScalar = VolumeScalar.Scale20;
            }
            else
            {
                PlayerVolumeScalar++;
            }

            _onUserInteraction?.Invoke();
        }

        private void SetVolumeAndPlaySound()
        {
            _propertiesManager.SetProperty(ApplicationConstants.PlayerVolumeScalarKey, PlayerVolumeScalar);
            if (_audio != null && _soundFile != null)
            {
                var volume = GetVolume(_audio);

                _audio.Play(_soundFile.Path, volume);
            }
        }

        private void LoadSoundFile()
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

                Logger.DebugFormat(
                    CultureInfo.CurrentCulture,
                    $"Found {SoundConfigurationExtensionPath} node: {node.FilePath}");

                files.Add(new SoundFileViewModel(name, path));
            }

            if (files.Count > 0)
            {
                _soundFile = files[0];
            }
        }
    }
}
