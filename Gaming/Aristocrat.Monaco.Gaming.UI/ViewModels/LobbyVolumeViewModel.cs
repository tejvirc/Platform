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
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;
    using log4net;

    public class LobbyVolumeViewModel : ObservableObject
    {
        private const string Volume0Key = "Volume0Normal";
        private const string Volume1Key = "Volume1Normal";
        private const string Volume2Key = "Volume2Normal";
        private const string Volume3Key = "Volume3Normal";
        private const string Volume4Key = "Volume4Normal";
        private const string Volume5Key = "Volume5Normal";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAudio _audio;
        private readonly Action _onUserInteraction;
        private VolumeScalar _playerVolumeScalar;

        public LobbyVolumeViewModel(Action onUserInteraction)
        {
            _onUserInteraction = onUserInteraction;
            _propertiesManager = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
            _audio = ServiceManager.GetInstance().TryGetService<IAudio>();

            _playerVolumeScalar = (VolumeScalar)_propertiesManager.GetValue(
                HardwareConstants.PlayerVolumeScalarKey,
                HardwareConstants.PlayerVolumeScalar);
            Logger.DebugFormat("Initializing default volume setting with value: {0}", PlayerVolumeScalar);
            VolumeCommand = new RelayCommand<object>(o => OnVolumeChange());
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
                    OnPropertyChanged(nameof(PlayerVolumeScalar));
                    OnPropertyChanged(nameof(VolumeValue));
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
                    HardwareConstants.LobbyVolumeScalarKey,
                    HardwareConstants.LobbyVolumeScalar);
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
            _propertiesManager.SetProperty(HardwareConstants.PlayerVolumeScalarKey, PlayerVolumeScalar);
            if (_audio != null)
            {
                var volume = GetVolume(_audio);

                _audio.Play(SoundName.Ding, volume);
            }
        }
    }
}
