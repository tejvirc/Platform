namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Models;

    [CLSCompliant(false)]
    public class VolumeViewModel : ObservableObject
    {
        protected readonly ILog Logger;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISystemDisableManager _disableManager;
        private readonly IEventBus _eventBus;
        private readonly IAudio _audioService;
        private bool _inputEnabled;
        private VolumeOption _selectedVolumeLevel;

        public ObservableCollection<VolumeOption> VolumeOptions { get; } = new();

        public VolumeViewModel()
        {
            Logger = LogManager.GetLogger(GetType());
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _audioService = ServiceManager.GetInstance().GetService<IAudio>();

            foreach (VolumeLevel volumeLevel in Enum.GetValues(typeof(VolumeLevel)))
            {
                VolumeOptions.Add(new VolumeOption(volumeLevel));
            }

            // Load default volume level
            var selectedVolumeLevel = (VolumeLevel)_propertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            _selectedVolumeLevel = VolumeOptions.FirstOrDefault(v => v.Level == selectedVolumeLevel);
            Logger.DebugFormat("Initializing default volume setting with value: {0}", _selectedVolumeLevel);
        }

        private bool IsAudioDisabled => !_audioService?.IsAvailable ?? true;

        private bool IsSystemDisabled =>
            _disableManager.CurrentDisableKeys.Contains(HardwareConstants.AudioDisconnectedLockKey) ||
            _disableManager.CurrentDisableKeys.Contains(HardwareConstants.AudioReconnectedLockKey);

        public bool InputEnabled
        {
            get => _inputEnabled;
            set => SetProperty(ref _inputEnabled, value, nameof(CanEditVolume));
        }

        public bool CanEditVolume => !IsAudioDisabled && !IsSystemDisabled && InputEnabled;

        public VolumeOption SelectedVolumeLevel
        {
            get => _selectedVolumeLevel;
            set
            {
                if (SetProperty(ref _selectedVolumeLevel, value))
                {
                    _propertiesManager.SetProperty(PropertyKey.DefaultVolumeLevel, (byte)value.Level);
                }
            }
        }

        public void OnLoaded()
        {
            UpdateVolumeOptions();

            if (IsSystemDisabled)
            {
                OnPropertyChanged(nameof(CanEditVolume));
            }

            OnPropertyChanged(nameof(SelectedVolumeLevel));

            _eventBus.Subscribe<EnabledEvent>(this, OnEnabledEvent);
            _eventBus.Subscribe<DisabledEvent>(this, OnDisabledEvent);
            _eventBus.Subscribe<OperatorCultureChangedEvent>(this, OnOperatorCultureChanged);
            LoadVolumeSettings();
        }

        private void LoadVolumeSettings()
        {
            // Load volume level
            var selectedVolumeLevel = (VolumeLevel)_propertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            SelectedVolumeLevel = VolumeOptions.FirstOrDefault(v => v.Level == selectedVolumeLevel);
        }

        public void OnUnloaded()
        {
            _eventBus.UnsubscribeAll(this);
        }

        private void OnEnabledEvent(EnabledEvent theEvent)
        {
            Execute.OnUIThread(() =>
            {
                OnPropertyChanged(nameof(CanEditVolume));
            });
        }

        private void OnDisabledEvent(DisabledEvent theEvent)
        {
            Execute.OnUIThread(() =>
            {
                OnPropertyChanged(nameof(CanEditVolume));
            });
        }

        private void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            UpdateVolumeOptions();
        }

        private void UpdateVolumeOptions()
        {
            foreach (var volumeOption in VolumeOptions)
            {
                volumeOption.UpdateDisplay();
            }
        }
    }
}
