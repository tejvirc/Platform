namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Aristocrat.Extensions.CommunityToolkit;
    using CommunityToolkit.Mvvm.Input;
    using ConfigWizard;
    using Aristocrat.Monaco.UI.Common.Extensions;
    using Aristocrat.Monaco.UI.Common.Markup;
    using Contracts;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Events;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Monaco.Kernel.Contracts;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class SoundConfigPageViewModel : InspectionWizardViewModelBase
    {
        private const bool IsAlertConfigurableDefault = false;
        private const bool ShowModeDefault = false;
        private const byte ShowModeAlertVolumeDefault = 100;
        private const byte ShowModeAlertVolumeMinimum = 70;
        private const byte AlertTickFrequency = 5;
        private const float VolumeToSliderFactor = 10f;
        private const float SliderToVolumeFactor = 100f;
        private const double SliderToVolumeSquareIndex = 2;

        private readonly IAudio _audio;
        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _propertiesManager;

        private byte _alertVolume;
        private byte _alertMinimumVolume;
        private string _infoText;
        private bool _playTestAlertSound;
        private string _soundFile;
        private bool _inTestMode;
        private byte _selectedVolumeLevel;

        public SoundConfigPageViewModel(bool isWizard) : base(isWizard)
        {
            _audio = ServiceManager.GetInstance().TryGetService<IAudio>();
            _disableManager = ServiceManager.GetInstance().TryGetService<ISystemDisableManager>();
            _propertiesManager = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
            TestViewModel.SetTestReporter(Inspection);
            ToggleTestModeCommand = new RelayCommand<object>(_ => InTestMode = !InTestMode);
            VolumeViewModel = new VolumeViewModel();
            //SoundTestCommand = new ActionCommand<object>(SoundTestClicked);
            SoundLevelConfigurationParser(_audio.SoundLevelCollection);
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
            var alertVolume = ConvertVolumeToSlider(
                PropertiesManager.GetValue(ApplicationConstants.AlertVolumeKey, ShowModeAlertVolumeDefault));
            _alertVolume = showMode
                ? alertVolume >= ShowModeAlertVolumeMinimum
                    ? alertVolume
                    : ShowModeAlertVolumeDefault
                : alertVolume;
            Logger.DebugFormat("Initializing alert volume setting with value: {0}", alertVolume);
            OnPropertyChanged(nameof(AlertVolume));

            IsAlertConfigurable = showMode || PropertiesManager.GetValue(ApplicationConstants.SoundConfigurationAlertVolumeConfigurable, IsAlertConfigurableDefault);
            OnPropertyChanged(nameof(IsAlertConfigurable));

            SelectedVolumeLevel = _propertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            OnPropertyChanged(nameof(SelectedVolumeLevel));
            //// Load default volume level
            //_soundLevel = PropertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            //Logger.DebugFormat("Initializing default volume setting with value: {0}", _soundLevel);
            //RaisePropertyChanged(nameof(SoundLevel));
        }

        public ObservableCollection<EnumerationExtension.EnumerationMember> SoundLevelConfigCollection { get; } = new();
        private void SoundLevelConfigurationParser(IEnumerable<Tuple<byte,string>> enumValues)
        {
            SoundLevelConfigCollection.Clear();
            foreach (var level in enumValues)
            {
                SoundLevelConfigCollection.Add(new EnumerationExtension.EnumerationMember()
                    {Description = level.Item2, Value = level.Item1});
            }
        }

        private bool IsSystemDisabled =>
            _disableManager.CurrentDisableKeys.Contains(HardwareConstants.AudioDisconnectedLockKey) ||
            _disableManager.CurrentDisableKeys.Contains(HardwareConstants.AudioReconnectedLockKey);

        private bool IsAudioDisabled => !_audio?.IsAvailable ?? true;

        public VolumeViewModel VolumeViewModel { get; }

        public int AlertMinimumVolume
        {
            get => _alertMinimumVolume;
            set => SetProperty(ref _alertMinimumVolume, (byte)value);
        }

        public static int AlertVolumeTickFrequency => AlertTickFrequency;

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

        public SoundTestPageViewModel TestViewModel { get; } = new SoundTestPageViewModel();

        public bool InTestMode
        {
            get => _inTestMode;
            set
            {
                if (_inTestMode == value)
                {
                    return;
                }

                TestViewModel.TestMode = value;
                if (!value)
                {
                    if (_inTestMode)
                    {
                        EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Sound));
                    }

                    UpdateStatusText();
                }
                else
                {
                    EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Sound));
                    EventBus.Publish(new OperatorMenuWarningMessageEvent(""));
                    if (TestViewModel?.SoundFiles?.Count > 0)
                    {
                        TestViewModel.Sound = TestViewModel.SoundFiles.First();
                    }
                }

                SetProperty(ref _inTestMode, value);
            }
        }

        public ICommand ToggleTestModeCommand { get; }

        public byte AlertVolume
        {
            get => _alertVolume;
            set
            {
                var scaledVolume = ConvertSliderToVolume(value);

                if (_alertVolume != value && value >= _alertMinimumVolume)
                {
                    _alertVolume = value;
                    OnPropertyChanged(nameof(AlertVolume));
                    PropertiesManager.SetProperty(ApplicationConstants.AlertVolumeKey, scaledVolume);
                }
                if (_playTestAlertSound)
                {
                    _audio.Play(_soundFile, scaledVolume);
                }
            }
        }
        public byte SelectedVolumeLevel
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

        protected override void OnLoaded()
        {
            EventBus.Subscribe<EnabledEvent>(this, OnEnabledEvent);
            EventBus.Subscribe<DisabledEvent>(this, OnDisabledEvent);

            LoadVolumeSettings();

            if (IsSystemDisabled)
            {
                OnPropertyChanged(nameof(TestModeEnabled));

                InfoText = Localizer.For(CultureFor.Operator).GetString(
                    _disableManager.CurrentDisableKeys.Contains(HardwareConstants.AudioReconnectedLockKey)
                        ? ResourceKeys.AudioConnected
                        : ResourceKeys.AudioDisconnect);
            }

            if (IsWizardPage)
            {
                InTestMode = true;
            }

            VolumeViewModel.OnLoaded();

            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            InTestMode = false;
            EventBus.UnsubscribeAll(this);

            VolumeViewModel.OnUnloaded();

            base.OnUnloaded();
        }

        protected override void SetupNavigation()
        {
            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateForward = true;
            }
        }

        protected override void SaveChanges()
        {
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
            VolumeViewModel.InputEnabled = InputEnabled;
        }

        private void OnEnabledEvent(EnabledEvent theEvent)
        {
            Execute.OnUIThread(() =>
            {
                OnPropertyChanged(nameof(TestModeEnabled));
                InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AudioConnected);
            });
        }

        private void OnDisabledEvent(DisabledEvent theEvent)
        {
            Execute.OnUIThread(() =>
            {
                OnPropertyChanged(nameof(TestModeEnabled));
                InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AudioDisconnect);
            });
        }

        public override bool TestModeEnabledSupplementary => !IsAudioDisabled && !IsSystemDisabled;

        public static byte ConvertSliderToVolume(byte sliderValue) =>
            (byte)(Math.Pow(sliderValue, SliderToVolumeSquareIndex) / SliderToVolumeFactor);


        public static byte ConvertVolumeToSlider(byte volume)
        {
            var sliderValue = Math.Sqrt(volume) * VolumeToSliderFactor;
            return (byte)(Math.Round(sliderValue / AlertTickFrequency) * AlertTickFrequency);
        }
    }
}
