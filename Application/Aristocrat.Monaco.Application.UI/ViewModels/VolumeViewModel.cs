namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Linq;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Audio;
    using Aristocrat.Monaco.Kernel.Contracts;
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

        public VolumeViewModel()
        {
            Logger = LogManager.GetLogger(GetType());
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _audioService = ServiceManager.GetInstance().GetService<IAudio>();

            // Load default volume level
            _selectedVolumeLevel = (VolumeLevel)_propertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
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
