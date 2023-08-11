namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Application.Settings;
    using Aristocrat.Extensions.CommunityToolkit;
    using CommunityToolkit.Mvvm.Input;
    using ConfigWizard;
    using Contracts;
    using Contracts.EdgeLight;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using EdgeLight;
    using Events;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class EdgeLightingPageViewModel : InspectionWizardViewModelBase
    {
        private const int MaxChannelBrightness = 100;
        private readonly IEdgeLightingController _edgeLightingController;
        private bool _isEdgeLightingAvailable;
        private int _brightnessSliderValue;
        private string _edgeLightingAttractModeOverrideSelection;
        private bool _bottomEdgeLightingOn;
        private bool _inTestMode;

        public EdgeLightingPageViewModel(bool isWizard) : base(isWizard)
        {
            _edgeLightingController = ServiceManager.GetInstance().GetService<IEdgeLightingController>();
            TestViewModel.SetTestReporter(Inspection);
            ToggleTestModeCommand = new RelayCommand<object>(_ => InTestMode = !InTestMode, _ => TestModeEnabled);
        }

        public ICommand ToggleTestModeCommand { get; }

        public string InfoText => IsEdgeLightingAvailable
            ? string.Empty
            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EdgeLightingDisconnectionText);

        public bool InfoTextVisible => !string.IsNullOrWhiteSpace(InfoText);

        public bool IsEdgeLightingAvailable
        {
            get => _isEdgeLightingAvailable;

            set
            {
                if (SetProperty(ref _isEdgeLightingAvailable, value, nameof(IsEdgeLightingAvailable)))
                {
                    OnPropertyChanged(nameof(TestButtonEnabled));
                }

                OnPropertyChanged(nameof(EdgeLightingEnabled));
                if (!value)
                {
                    InTestMode = false;
                }
            }
        }

        public bool EdgeLightingEnabled => InputEnabled && IsEdgeLightingAvailable;

        public bool TestButtonEnabled => TestModeEnabled && IsEdgeLightingAvailable;

        public bool InTestMode
        {
            get => _inTestMode;
            set
            {
                TestViewModel.TestMode = value;
                if (!value)
                {
                    if (_inTestMode)
                    {
                        EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.EdgeLighting));
                    }

                    _edgeLightingController.ClearBrightnessForPriority(StripPriority.PlatformTest);
                    UpdateStatusText();
                }
                else
                {
                    EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.EdgeLighting));
                    EventBus.Publish(new OperatorMenuWarningMessageEvent(""));
                }

                SetProperty(ref _inTestMode, value, nameof(InTestMode));
            }
        }

        public override bool TestModeEnabled
        {
            get => base.TestModeEnabled;
            set
            {
                base.TestModeEnabled = value;
                if (ToggleTestModeCommand is IRelayCommand RelayCommand)
                {
                    Execute.OnUIThread(() => RelayCommand.NotifyCanExecuteChanged());
                }
            }
        }

        public bool IsCabinetThatAllowsBottomStripToggle =>
            PropertiesManager.GetValue(ApplicationConstants.BottomStripEnabled, false);

        public bool ShowOverrideSetting =>
            GetConfigSetting(OperatorMenuSetting.EdgeLightingOverrideVisible, true);

        public bool IsCabinetThatAllowsEdgeLightBrightnessSetting =>
            PropertiesManager.GetValue(ApplicationConstants.EdgeLightingBrightnessControlEnabled, false);

        public int MinimumBrightnessSetting =>
            PropertiesManager.GetValue(ApplicationConstants.EdgeLightingBrightnessControlMin, 0);

        public int MaximumBrightnessSetting =>
            PropertiesManager.GetValue(ApplicationConstants.EdgeLightingBrightnessControlMax, 100);

        public int MaximumAllowedBrightnessValue
        {
            get => _brightnessSliderValue;
            set
            {
                SetProperty(ref _brightnessSliderValue, value, nameof(MaximumAllowedBrightnessValue));
                PropertiesManager.SetProperty(
                    ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey,
                    _brightnessSliderValue);
                EventBus.Publish
                    (new MaximumOperatorBrightnessChangedEvent(_brightnessSliderValue));
            }
        }

        public bool BottomEdgeLightingOn
        {
            get => _bottomEdgeLightingOn;
            set
            {
                SetProperty(ref _bottomEdgeLightingOn, value, nameof(BottomEdgeLightingOn));
                PropertiesManager.SetProperty(ApplicationConstants.BottomEdgeLightingOnKey, _bottomEdgeLightingOn);
                if (_bottomEdgeLightingOn)
                {
                    EventBus.Publish(new BottomStripOnEvent());
                }
                else
                {
                    EventBus.Publish(new BottomStripOffEvent());
                }
            }
        }

        // TODO: need to set the values based on jurisdiction
        public Dictionary<string, string> LightingOverrideChoices =>
            new Dictionary<string, string>
            {
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideTransparent),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideDisabled)
                },
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideBlue),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideBlue)
                },
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideRed),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideRed)
                },
                {
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideGold),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideGold)
                }
            };

        public string EdgeLightingAttractModeOverrideSelection
        {
            get => _edgeLightingAttractModeOverrideSelection;
            set
            {
                SetProperty(ref _edgeLightingAttractModeOverrideSelection, value, nameof(EdgeLightingAttractModeOverrideSelection));
                PropertiesManager.SetProperty(
                    ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
                    _edgeLightingAttractModeOverrideSelection);
            }
        }

        public EdgeLightingTestViewModel TestViewModel { get; } = new EdgeLightingTestViewModel();

        protected override void OnLoaded()
        {
            IsEdgeLightingAvailable = _edgeLightingController.IsDetected || (bool)PropertiesManager.GetProperty(KernelConstants.IsInspectionOnly, false);

            if (IsEdgeLightingAvailable && IsWizardPage)
            {
                InTestMode = true;
            }

            EventBus.Subscribe<EdgeLightingConnectedEvent>(this, HandleEdgeLightConnectedEvent);
            EventBus.Subscribe<EdgeLightingDisconnectedEvent>(this, HandleEdgeLightDisconnectedEvent);
            EventBus.Subscribe<OperatorCultureChangedEvent>(this, HandleOperatorCultureChangedEvent);

            if (IsCabinetThatAllowsEdgeLightBrightnessSetting)
            {
                MaximumAllowedBrightnessValue = PropertiesManager.GetValue(
                    ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey,
                    PropertiesManager.GetValue(
                        ApplicationConstants.EdgeLightingBrightnessControlDefault,
                        MaxChannelBrightness));
            }

            EdgeLightingAttractModeOverrideSelection = PropertiesManager.GetValue(
                ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideTransparent));

            if (IsCabinetThatAllowsBottomStripToggle)
            {
                BottomEdgeLightingOn = PropertiesManager.GetValue(ApplicationConstants.BottomEdgeLightingOnKey, false);
            }

            Inspection?.SetFirmwareVersion(
                MachineSettingsUtilities.GetLightingIdentification(Localizer.For(CultureFor.Operator)));

            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            InTestMode = false;
            _edgeLightingController.ClearBrightnessForPriority(StripPriority.PlatformTest);
            EventBus.UnsubscribeAll(this);

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

        protected override void OnInputEnabledChanged()
        {
            OnPropertyChanged(nameof(EdgeLightingEnabled));
        }

        protected override void OnTestModeEnabledChanged()
        {
            OnPropertyChanged(nameof(TestButtonEnabled));
        }

        protected override void UpdateStatusText()
        {
            if (InfoTextVisible && !string.IsNullOrEmpty(InfoText))
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(InfoText));
            }
            else
            {
                base.UpdateStatusText();
            }
        }

        private void HandleEdgeLightConnectedEvent(EdgeLightingConnectedEvent evt)
        {
            Execute.OnUIThread(
                () =>
                {
                    IsEdgeLightingAvailable = true;
                    OnPropertyChanged(nameof(InfoText), nameof(InfoTextVisible));
                    UpdateStatusText();
                });
        }

        private void HandleEdgeLightDisconnectedEvent(EdgeLightingDisconnectedEvent evt)
        {
            Execute.OnUIThread(
                () =>
                {
                    IsEdgeLightingAvailable = false;
                    OnPropertyChanged(nameof(InfoText), nameof(InfoTextVisible));
                    UpdateStatusText();
                });
        }

        private void HandleOperatorCultureChangedEvent(OperatorCultureChangedEvent evt)
        {
            Execute.OnUIThread(
                () =>
                {
                    OnPropertyChanged(nameof(InfoText));
                    UpdateStatusText();
                });
        }
    }
}
