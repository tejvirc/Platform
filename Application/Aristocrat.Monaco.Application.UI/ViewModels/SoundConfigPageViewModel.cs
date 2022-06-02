namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Aristocrat.Monaco.UI.Common.Extensions;
    using Aristocrat.Monaco.UI.Common.Markup;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using MVVM;
    using MVVM.Command;
    using OperatorMenu;
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
            SoundTestCommand = new ActionCommand<object>(SoundTestClicked);
            SoundLevelConfigurationParser(_audio.SoundLevelCollection);
        }

        private void LoadVolumeSettings()
        {
            var showMode = PropertiesManager.GetValue(ApplicationConstants.ShowMode, ShowModeDefault);

            AlertMinimumVolume = showMode
                ? ShowModeAlertVolumeMinimum
                : PropertiesManager.GetValue(ApplicationConstants.SoundConfigurationAlertVolumeMinimum, ApplicationConstants.AlertVolumeMinimum);
            RaisePropertyChanged(nameof(AlertMinimumVolume));

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
            RaisePropertyChanged(nameof(AlertVolume));

            IsAlertConfigurable = showMode || PropertiesManager.GetValue(ApplicationConstants.SoundConfigurationAlertVolumeConfigurable, IsAlertConfigurableDefault);
            RaisePropertyChanged(nameof(IsAlertConfigurable));

            // Load default volume level
            _soundLevel = (VolumeLevel)PropertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            Logger.DebugFormat("Initializing default volume setting with value: {0}", _soundLevel);
            RaisePropertyChanged(nameof(SoundLevel));
        }

        public bool CanEditVolume => !IsAudioDisabled && !IsSystemDisabled && InputEnabled;

        public ObservableCollection<EnumerationExtension.EnumerationMember> SoundLevelConfigCollection { get; } = new();
        private void SoundLevelConfigurationParser(IEnumerable<VolumeLevel> enumValues)
        {
            SoundLevelConfigCollection.Clear();
            foreach (var level in enumValues)
            {
                SoundLevelConfigCollection.Add(new EnumerationExtension.EnumerationMember()
                    {Description = level.GetDescription(typeof(VolumeLevel)), Value = level});
            }
        }

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
                RaisePropertyChanged(nameof(InfoText));
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
                RaisePropertyChanged(nameof(SoundLevel));
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
                    RaisePropertyChanged(nameof(AlertVolume));
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
                RaisePropertyChanged(nameof(CanEditVolume), nameof(TestModeEnabled));

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
            RaisePropertyChanged(nameof(CanEditVolume));
        }

        private void OnEnabledEvent(EnabledEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                RaisePropertyChanged(nameof(CanEditVolume), nameof(TestModeEnabled));
                InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AudioConnected);
            });
        }

        private void OnDisabledEvent(DisabledEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                RaisePropertyChanged(nameof(CanEditVolume), nameof(TestModeEnabled));
                InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AudioDisconnect);
            });
        }

        public override bool TestModeEnabledSupplementary => CanEditVolume;
    }
}