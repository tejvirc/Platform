namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Xml.Linq;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Audio;
    using Aristocrat.Monaco.Hardware.Contracts.EdgeLighting;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Aristocrat.Monaco.Localization.Properties;
    using Contracts;
    using Kernel;
    using log4net;
    using MVVM;
    using MVVM.ViewModel;

    [CLSCompliant(false)]
    public class VolumeViewModel : BaseViewModel
    {
        protected new readonly ILog Logger;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISystemDisableManager _disableManager;
        private readonly IEventBus _eventBus;
        private readonly IAudio _audioService;
        private bool _inputEnabled;
        private VolumeLevel _selectedVolumeLevel;

        private ObservableCollection<string> volumeOptions = new ObservableCollection<string>() {
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExtraLow),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Low),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediumLow),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Medium),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediumHigh),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.High),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExtraHigh),
        };

        public ObservableCollection<string> VolumeOptions   // property
        {
            get { return volumeOptions; }   // get method
            set { volumeOptions = value; }  // set method
        }

        public VolumeViewModel()
        {
            Logger = LogManager.GetLogger(GetType());
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _audioService = ServiceManager.GetInstance().GetService<IAudio>();

            // Load default volume level
            SelectedVolumeLevel = (VolumeLevel)_propertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
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

        public VolumeLevel SelectedVolumeLevel
        {
            get => _selectedVolumeLevel;

            set
            {
                if (SetProperty(ref _selectedVolumeLevel, value))
                {
                    _propertiesManager.SetProperty(PropertyKey.DefaultVolumeLevel, (byte)value);
                }
            }
        }

        public void OnLoaded()
        {
            if (IsSystemDisabled)
            {
                RaisePropertyChanged(nameof(CanEditVolume));
            }

            RaisePropertyChanged(nameof(SelectedVolumeLevel));

            _eventBus.Subscribe<EnabledEvent>(this, OnEnabledEvent);
            _eventBus.Subscribe<DisabledEvent>(this, OnDisabledEvent);
            LoadVolumeSettings();
        }

        private void LoadVolumeSettings()
        {
            // Load volume level
            SelectedVolumeLevel = (VolumeLevel)_propertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            RaisePropertyChanged(nameof(SelectedVolumeLevel));
        }

        public void OnUnloaded()
        {
            _eventBus.UnsubscribeAll(this);
        }

        private void OnEnabledEvent(EnabledEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                RaisePropertyChanged(nameof(CanEditVolume));
            });
        }

        private void OnDisabledEvent(DisabledEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                RaisePropertyChanged(nameof(CanEditVolume));
            });
        }
    }
}
