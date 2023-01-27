namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Timers;
    using System.Windows.Input;
    using ConfigWizard;
    using Contracts;
    using Contracts.HardwareDiagnostics;
    using Contracts.LampTest;
    using Contracts.Localization;
    using Events;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Kernel.Contracts;
    using LampTest;
    using Models;
    using Monaco.Localization.Properties;
    using MVVM.Command;
    using Timer = System.Timers.Timer;

    [CLSCompliant(false)]
    public class LampsPageViewModel : InspectionWizardViewModelBase
    {
        private bool _flashState;
        private Timer _flashTimer;
        private int? _selectedInterval;
        private string _selectedButtonLamp;
        private TowerLight _selectedTowerLight;
        private readonly ILampTest _lampTest;
        private readonly ITowerLight _towerLight;
        private readonly ITowerLightManager _towerLightManager;
        private readonly List<FlashState> _allFlashStates = new List<FlashState>();
        private readonly List<FlashState> _strobeFlashStates = new List<FlashState>();

        public LampsPageViewModel(bool isWizard) : base(isWizard)
        {
            ButtonLampsAvailable = ButtonDeckUtilities.HasLamps();

            _lampTest = LampTestUtilities.GetLampTest();

            _towerLightManager = ServiceManager.GetInstance().TryGetService<ITowerLightManager>();
            _towerLight = ServiceManager.GetInstance().TryGetService<ITowerLight>();

            TowerLightsEnabled = !(_towerLightManager?.TowerLightsDisabled ?? true) || (bool)PropertiesManager.GetProperty(KernelConstants.IsInspectionOnly, false);
            TowerLights = new List<TowerLight>();
            _allFlashStates.AddRange((FlashState[])Enum.GetValues(typeof(FlashState)));
            _strobeFlashStates.AddRange(new []{ FlashState.Off, FlashState.On });

            var tiers = _towerLightManager?.ConfiguredLightTiers?.ToList();
            if (tiers != null)
            {
                foreach (var tier in tiers)
                {
                    var state = _towerLight?.GetFlashState(tier);
                    var lamp = new TowerLight(tier, state.GetValueOrDefault(FlashState.Off));
                    TowerLights.Add(lamp);
                }
            }

            _selectedTowerLight = TowerLights.FirstOrDefault();

            SetTowerLightFlashStateCommand = new ActionCommand<object>(SetTowerLightFlashState);
        }

        public bool ButtonLampsAvailable { get; set; }

        public bool TowerLightsEnabled { get; set; }

        public ObservableCollection<int> Intervals { get; } = new ObservableCollection<int>();

        public ObservableCollection<string> ButtonLamps { get; } = new ObservableCollection<string>();

        public ICommand SetTowerLightFlashStateCommand { get; }

        public List<TowerLight> TowerLights { get; set; }

        public TowerLight SelectedTowerLight
        {
            get => _selectedTowerLight;

            set
            {
                if (_selectedTowerLight != value)
                {
                    _selectedTowerLight = value;
                    SelectedFlashState = _selectedTowerLight.FlashState;

                    RaisePropertyChanged(nameof(SelectedTowerLight));
                    RaisePropertyChanged(nameof(SelectedFlashState));
                    RaisePropertyChanged(nameof(FlashStates));
                }
            }
        }

        public List<FlashState> FlashStates =>
            SelectedTowerLight?.Tier == LightTier.Strobe ? _strobeFlashStates : _allFlashStates;

        public FlashState SelectedFlashState { get; set; }

        public string SelectedButtonLamp
        {
            get => _selectedButtonLamp;

            set
            {
                if (_selectedButtonLamp == value)
                {
                    return;
                }

                _selectedButtonLamp = value;
                RaisePropertyChanged(nameof(SelectedButtonLamp));
                SetSelectedLamps(value);
            }
        }

        public int? SelectedInterval
        {
            get => _selectedInterval;

            set
            {
                if (_selectedInterval == value)
                {
                    return;
                }

                StopFlashTimer();

                _selectedInterval = value;
                RaisePropertyChanged(nameof(SelectedInterval));

                if (SelectedInterval != null)
                {
                    StartFlashTimer(SelectedInterval.Value);
                }
                else
                {
                    _lampTest?.SetLampState(true);
                }
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            _lampTest?.SetEnabled(true);

            EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Lamps));

            // Clean Collections
            Intervals.Clear();
            ButtonLamps.Clear();

            Intervals.Add(125);
            Intervals.Add(250);
            Intervals.Add(500);

            ButtonLamps.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BashButtonLamp));
            ButtonLamps.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TestAllButtonLamps));

            if (TowerLightsEnabled)
            {
                EventBus.Subscribe<TowerLightOffEvent>(
                    this,
                    evt => HandleTowerLightEvent(evt.LightTier, false, evt.FlashState));
                EventBus.Subscribe<TowerLightOnEvent>(
                    this,
                    evt => HandleTowerLightEvent(evt.LightTier, true, evt.FlashState));

                var tiers = _towerLightManager?.ConfiguredLightTiers?.ToList();
                if (tiers != null)
                {
                    foreach (var tier in tiers)
                    {
                        var state = _towerLight?.GetFlashState(tier);
                        var towerLight = TowerLights.FirstOrDefault(t => t.Tier == tier);
                        if (towerLight != null && state.HasValue)
                        {
                            towerLight.FlashState = state.Value;
                        }
                    }
                }
            }
        }

        protected override void OnInputEnabledChanged()
        {
            if (!InputEnabled)
            {
                SelectedInterval = null;
                SelectedButtonLamp = null;
            }
        }

        protected override void DisposeInternal()
        {
            StopFlashTimer();
            EventBus.UnsubscribeAll(this);
            base.DisposeInternal();
        }

        protected override void OnUnloaded()
        {
            _towerLightManager?.ReStart();
            StopFlashTimer();

            EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Lamps));

            _lampTest?.SetEnabled(false);

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

        protected override void OnTestModeEnabledChanged()
        {
            UpdateStatusText();

            if (!TestModeEnabled)
            {
                SelectedButtonLamp = null;
                SelectedInterval = null;
                _lampTest?.SetEnabled(false);
            }
        }

        protected override void UpdateStatusText()
        {
            if (!TestModeEnabled)
            {
                if (!string.IsNullOrEmpty(TestWarningText))
                {
                    EventBus.Publish(new OperatorMenuWarningMessageEvent(TestWarningText));
                }
                else
                {
                    base.UpdateStatusText();
                }
            }
            else
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent());
            }
        }

        private void StartFlashTimer(int interval)
        {
            StopFlashTimer();

            _flashTimer = new Timer(interval) { AutoReset = true };
            _flashTimer.Elapsed += OnFlashTick;
            _flashTimer.Start();
        }

        private void StopFlashTimer()
        {
            _flashTimer?.Stop();
            _flashTimer?.Dispose();
            _flashTimer = null;
        }

        private void OnFlashTick(object sender, ElapsedEventArgs args)
        {
            _flashState = !_flashState;
            _lampTest?.SetLampState(_flashState);
        }

        private void SetSelectedLamps(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                Inspection?.SetTestName(s);
            }
            var selectedLamp = SelectedLamps.None;
            if (string.Equals(s, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BashButtonLamp)))
            {
                selectedLamp = SelectedLamps.Bash;
            }
            else if (string.Equals(s, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TestAllButtonLamps)))
            {
                selectedLamp = SelectedLamps.All;
            }
            _lampTest?.SetSelectedLamps(selectedLamp, SelectedInterval is null);
        }

        private void SetTowerLightFlashState(object parameter)
        {
            _towerLight?.SetFlashState(SelectedTowerLight?.Tier ?? LightTier.Tier1, SelectedFlashState, Timeout.InfiniteTimeSpan);
            Inspection?.SetTestName($"{SelectedTowerLight} {SelectedFlashState}");
        }

        private void HandleTowerLightEvent(LightTier lightTier, bool lightOn, FlashState flashState)
        {
            var towerLight = TowerLights.FirstOrDefault(t => t.Tier == lightTier);
            if (towerLight != null)
            {
                towerLight.FlashState = flashState;
                towerLight.State = lightOn;
            }
        }

        private bool HasTowerLight(int tier)
        {
            return TowerLights.Any(t => t.Tier == (LightTier)tier);
        }
    }
}
