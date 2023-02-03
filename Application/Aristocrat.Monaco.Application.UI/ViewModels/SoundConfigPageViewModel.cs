namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Linq;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using OperatorMenu;
    using Toolkit.Mvvm.Extensions;
    using Views;

    [CLSCompliant(false)]
    public class SoundConfigPageViewModel : OperatorMenuPageViewModelBase
    {
        private const bool IsAlertConfigurableDefault = false;
        private const bool ShowModeDefault = false;
        private const byte ShowModeAlertVolumeDefault = 100;
        private const byte ShowModeAlertVolumeMinimum = 70;

        private readonly IAudio _audio;
        private readonly ISystemDisableManager _disableManager;

        private VolumeLevel _soundLevel;
        private byte _alertVolume;
        private byte _alertMinimumVolume;
        private string _infoText;
        private bool _playTestAlertSound;
        private string _soundFile;

        public SoundConfigPageViewModel()
        {
            _audio = ServiceManager.GetInstance().TryGetService<IAudio>();
            _disableManager = ServiceManager.GetInstance().TryGetService<ISystemDisableManager>();
            SoundTestCommand = new RelayCommand<object>(SoundTestClicked);
        }

        private void LoadVolumeSettings()
        {
            var showMode = PropertiesManager.GetValue(ApplicationConstants.ShowMode, ShowModeDefault);

            AlertMinimumVolume = showMode
                ? ShowModeAlertVolumeMinimum
                : PropertiesManager.GetValue(ApplicationConstants.SoundConfigurationAlertVolumeMinimum, ApplicationConstants.AlertVolumeMinimum);
            OnPropertyChanged(nameof(AlertMinimumVolume));

            _playTestAlertSound = PropertiesManager.GetValue(ApplicationConstants.SoundConfigurationPlayTestAlertSound, ApplicationConstants.DefaultPlayTestAlertSound);
            _soundFile = PropertiesManager.GetValue(ApplicationConstants.DingSoundKey, "");

            // Load alert volume level and settings
            var alertVolume = PropertiesManager.GetValue(ApplicationConstants.AlertVolumeKey, ShowModeAlertVolumeDefault);
            _alertVolume = showMode
                ? alertVolume >= ShowModeAlertVolumeMinimum
                    ? alertVolume
                    : ShowModeAlertVolumeDefault
                : alertVolume;
            Logger.DebugFormat("Initializing alert volume setting with value: {0}", alertVolume);
            OnPropertyChanged(nameof(AlertVolume));

            IsAlertConfigurable = showMode || PropertiesManager.GetValue(ApplicationConstants.SoundConfigurationAlertVolumeConfigurable, IsAlertConfigurableDefault);
            OnPropertyChanged(nameof(IsAlertConfigurable));

            // Load default volume level
            _soundLevel = (VolumeLevel)PropertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            Logger.DebugFormat("Initializing default volume setting with value: {0}", _soundLevel);
            OnPropertyChanged(nameof(SoundLevel));
        }

        public bool CanEditVolume => !IsAudioDisabled && !IsSystemDisabled && InputEnabled;

        private bool IsSystemDisabled =>
            _disableManager.CurrentDisableKeys.Contains(HardwareConstants.AudioDisconnectedLockKey) ||
            _disableManager.CurrentDisableKeys.Contains(HardwareConstants.AudioReconnectedLockKey);

        private bool IsAudioDisabled => !_audio?.IsAvailable ?? true;

        public int AlertMinimumVolume
        {
            get => _alertMinimumVolume;
            set => SetProperty(ref _alertMinimumVolume, (byte)value, nameof(AlertMinimumVolume));
        }

        public string InfoText
        {
            get => _infoText;
            set
            {
                _infoText = value;
                OnPropertyChanged(nameof(InfoText));
                UpdateStatusText();
            }
        }

        public bool IsAlertConfigurable { get; private set; }

        private void SoundTestClicked(object obj)
        {
            var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();

            var viewModel = new SoundTestPageViewModel();

            dialogService.ShowInfoDialog<SoundTestPage>(
                 this,
                 viewModel,
                 Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SoundTest));
        }

        public VolumeLevel SoundLevel
        {
            get => _soundLevel;

            set
            {
                if (_soundLevel == value)
                {
                    return;
                }

                _soundLevel = value;
                OnPropertyChanged(nameof(SoundLevel));
                PropertiesManager.SetProperty(PropertyKey.DefaultVolumeLevel, (byte)value);
            }
        }

        public System.Windows.Input.ICommand SoundTestCommand { get; }

        public byte AlertVolume
        {
            get => _alertVolume;
            set
            {
                if (_alertVolume != value && value >= _alertMinimumVolume)
                {
                    _alertVolume = value;
                    OnPropertyChanged(nameof(AlertVolume));
                    PropertiesManager.SetProperty(ApplicationConstants.AlertVolumeKey, value);
                }
                if (_playTestAlertSound)
                {
                    _audio.Play(_soundFile, value);
                }
            }
        }

        protected override void OnLoaded()
        {
            EventBus.Subscribe<EnabledEvent>(this, OnEnabledEvent);
            EventBus.Subscribe<DisabledEvent>(this, OnDisabledEvent);

            LoadVolumeSettings();

            if (IsSystemDisabled)
            {
                OnPropertyChanged(nameof(CanEditVolume));
                OnPropertyChanged(nameof(TestModeEnabled));

                InfoText = Localizer.For(CultureFor.Operator).GetString(
                    _disableManager.CurrentDisableKeys.Contains(HardwareConstants.AudioReconnectedLockKey)
                        ? ResourceKeys.AudioConnected
                        : ResourceKeys.AudioDisconnect);
            }
        }

        protected override void UpdateStatusText()
        {
            if (!string.IsNullOrEmpty(InfoText))
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(InfoText));
            }
            else
            {
                base.UpdateStatusText();
            }
        }

        protected override void OnInputEnabledChanged()
        {
            OnPropertyChanged(nameof(CanEditVolume));
        }

        private void OnEnabledEvent(EnabledEvent theEvent)
        {
            Execute.OnUIThread(() =>
            {
                OnPropertyChanged(nameof(CanEditVolume));
                OnPropertyChanged(nameof(TestModeEnabled));
                InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AudioConnected);
            });
        }

        private void OnDisabledEvent(DisabledEvent theEvent)
        {
            Execute.OnUIThread(() =>
            {
                OnPropertyChanged(nameof(CanEditVolume));
                OnPropertyChanged(nameof(TestModeEnabled));
                InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AudioDisconnect);
            });
        }

        public override bool TestModeEnabledSupplementary => CanEditVolume;
    }
}