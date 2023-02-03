namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Models;
    using Monaco.Localization.Properties;
    using Toolkit.Mvvm.Extensions;

    /// <summary>
    ///     View model for mechanical reels
    /// </summary>
    [CLSCompliant(false)]
    public class MechanicalReelsPageViewModel : DeviceViewModel
    {
        private const int MaxSupportedReels = ReelConstants.MaxSupportedReels;

        private string _reelCount;

        private bool _reelsEnabled;
        private bool _lightTestScreenHidden = true;
        private bool _reelTestScreenHidden = true;
        private bool _settingsScreenHidden;
        private bool _selfTestEnabled = true;

        public MechanicalReelsPageViewModel() : base(DeviceType.ReelController)
        {
            ShowLightTestCommand = new RelayCommand<object>(_ =>
            {
                LightTestScreenHidden = false;
                ReelTestScreenHidden = true;
                SettingsScreenHidden = true;
            });
            ShowReelTestCommand = new RelayCommand<object>(_ =>
            {
                LightTestScreenHidden = true;
                ReelTestScreenHidden = false;
                SettingsScreenHidden = true;
            });
            ShowSettingsCommand = new RelayCommand<object>(_ =>
            {
                LightTestScreenHidden = true;
                ReelTestScreenHidden = true;
                SettingsScreenHidden = false;
            });

            var edgeLightController = ServiceManager.GetInstance().GetService<IEdgeLightingController>();
            LightTestViewModel = new(ReelController, edgeLightController);
            ReelTestViewModel = new(ReelController, EventBus, MaxSupportedReels, ReelInfo, UpdateScreen);

            SelfTestCommand = new RelayCommand<object>(_ => SelfTest(false));
            SelfTestClearCommand = new RelayCommand<object>(_ => SelfTest(true));

            MinimumBrightness = 1;
            MaximumBrightness = 100;
        }

        public MechanicalReelsLightTestViewModel LightTestViewModel { get; }

        public MechanicalReelsTestViewModel ReelTestViewModel { get; }

        public bool ReelTestScreenHidden
        {
            get => _reelTestScreenHidden;

            set
            {
                _reelTestScreenHidden = value;
                OnPropertyChanged(nameof(ReelTestScreenHidden));
                OnPropertyChanged(nameof(ReelTestButtonHidden));
            }
        }

        public bool ReelTestButtonHidden => !ReelTestScreenHidden;

        public bool LightTestScreenHidden
        {
            get => _lightTestScreenHidden;

            set
            {
                _lightTestScreenHidden = value;
                OnPropertyChanged(nameof(LightTestScreenHidden));
                OnPropertyChanged(nameof(LightTestButtonHidden));
            }
        }

        public bool LightTestButtonHidden => !LightTestScreenHidden;

        public bool SettingsScreenHidden
        {
            get => _settingsScreenHidden;

            set
            {
                _settingsScreenHidden = value;
                OnPropertyChanged(nameof(SettingsScreenHidden));
                OnPropertyChanged(nameof(SettingsButtonHidden));
            }
        }

        public bool SettingsButtonHidden => !SettingsScreenHidden;

        public string ReelCount
        {
            get => _reelCount;

            set
            {
                _reelCount = value;
                OnPropertyChanged(nameof(ReelCount));
                OnPropertyChanged(nameof(ReelCountForeground));
            }
        }

        public bool ReelsEnabled
        {
            get => _reelsEnabled;
            set
            {
                if (_reelsEnabled != value)
                {
                    _reelsEnabled = value;
                    OnPropertyChanged(nameof(ReelsEnabled));
                }
            }
        }

        public SolidColorBrush ReelCountForeground => Brushes.White;

        public ICommand ShowLightTestCommand { get; }

        public ICommand ShowReelTestCommand { get; }

        public ICommand ShowSettingsCommand { get; }

        public ICommand SelfTestCommand { get; }

        public ICommand SelfTestClearCommand { get; }

        public int MinimumBrightness { get; }

        public int MaximumBrightness { get; }

        public override bool TestModeEnabledSupplementary => ReelController?.Connected ?? false;

        public bool TestModeToolTipDisabled => TestModeEnabled;

        public ObservableCollection<ReelInfoItem> ReelInfo { get; set; } = new();

        public int Brightness
        {
            get => ReelController?.DefaultReelBrightness ?? MaximumBrightness;

            set
            {
                ReelController.DefaultReelBrightness = value;
                ReelController.SetReelBrightness(value);
                OnPropertyChanged(nameof(Brightness));
            }
        }

        public bool SelfTestEnabled
        {
            get => _selfTestEnabled;

            set
            {
                if (_selfTestEnabled == value)
                {
                    return;
                }

                _selfTestEnabled = value;
                OnPropertyChanged(nameof(SelfTestEnabled));
            }
        }

        protected override void DisposeInternal()
        {
            LightTestViewModel.Dispose();
            base.DisposeInternal();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            UpdateScreen();

            UpdateWarningMessage();

            Brightness = ReelController?.DefaultReelBrightness ?? MaximumBrightness;
        }

        protected override void OnUnloaded()
        {
            LightTestViewModel.CancelTest();
            EventBus.UnsubscribeAll(this);
            base.OnUnloaded();
        }

        protected override void OnTestModeEnabledChanged()
        {
            OnPropertyChanged(nameof(TestModeToolTipDisabled));

            LightTestScreenHidden = true;
            ReelTestScreenHidden = true;
            SettingsScreenHidden = false;
        }

        protected void SetDeviceInformation()
        {
            if (ReelController == null)
            {
                return;
            }

            SetDeviceInformation(ReelController.DeviceConfiguration);

            var connectedReels = ReelController.ConnectedReels;
            ReelCount = connectedReels.Count.ToString();

            var offsets = ReelController.ReelOffsets.ToArray();

            for (int i = 1; i <= MaxSupportedReels; ++i)
            {
                if (connectedReels.Contains(i))
                {
                    if (IsReelActive(i) == false)
                    {
                        ReelInfo.Add(
                            new ReelInfoItem(
                                i,
                                true,
                                true,
                                ReelLogicalState.Disconnected.ToString(),
                                offsets.Length >= i ? offsets[i - 1] : 0));
                    }
                    else
                    {
                        GetActiveReel(i).Connected = true;
                    }
                }
                else
                {
                    if (IsReelActive(i))
                    {
                        GetActiveReel(i).Connected = false;
                    }
                }
            }

            ReelTestViewModel.ReelInfo = ReelInfo;
        }

        protected override void StartEventHandler()
        {
            StartEventHandler(ReelController);
        }

        protected override void SubscribeToEvents()
        {
            EventBus.Subscribe<DisconnectedEvent>(this, HandleEvent);
            EventBus.Subscribe<ConnectedEvent>(this, HandleEvent);
            EventBus.Subscribe<ReelStoppedEvent>(this, HandleEvent);
            EventBus.Subscribe<HardwareFaultEvent>(this, HandleEvent);
            EventBus.Subscribe<HardwareReelFaultEvent>(this, HandleEvent);
            EventBus.Subscribe<PropertyChangedEvent>(this, HandleEvent);
            EventBus.Subscribe<HardwareDiagnosticTestStartedEvent>(this, HandleEvent);
            EventBus.Subscribe<HardwareDiagnosticTestFinishedEvent>(this, HandleEvent);
        }

        protected override void UpdateScreen()
        {
            Execute.OnUIThread(
                () =>
                {
                    SetDeviceInformation();
                    SetReelControllerStatus();
                    SetReelsState();
                    ReelTestViewModel.UpdateScreen();
                });
        }

        protected override void UpdateWarningMessage()
        {
            if (!(ReelController?.Connected ?? false) || (ReelController?.DisabledByError ?? false))
            {
                TestWarningText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TestModeDisabledStatusDevice);
            }
            else
            {
                base.UpdateWarningMessage();
            }

            OnPropertyChanged(nameof(TestModeToolTipDisabled));
        }

        private async void SelfTest(bool clearNvm)
        {
            SelfTestEnabled = false;

            await Task.Run(() =>
            {
                ReelController.SelfTest(clearNvm);
            });

            SelfTestEnabled = true;
        }

        private IReelController ReelController => ServiceManager.GetInstance().TryGetService<IReelController>();

        private bool IsHomed { get; set; }

        private ReelInfoItem GetActiveReel(int reel) => ReelInfo.First(o => o.Id == reel);

        private string GetState(string state, int reelId = 0)
        {
            var localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReelController_IdleUnknown);

            if (state == ReelControllerState.IdleAtStops.ToString() ||
                state == ReelLogicalState.IdleAtStop.ToString())
            {
                localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Idle);
            }
            else if (state == ReelControllerState.Spinning.ToString())
            {
                localizedState = ReelInfo.Any(o => o.IsHoming) ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Homing) :
                    ReelInfo.Any(o => o.IsNudging) ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Nudging) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Spinning);
            }
            else if (reelId > 0)
            {
                var activeReel = GetActiveReel(reelId);

                if (state == ReelControllerState.Disconnected.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disconnected);
                }
                else if ((activeReel.IsHoming || state == ReelControllerState.Homing.ToString()) && state != ReelControllerState.Tilted.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Homing);

                    var reelsStatus = ReelController.ReelsStatus;
                    if (reelsStatus.ContainsKey(reelId))
                    {
                        reelsStatus.TryGetValue(reelId, out var reelStatus);
                        if (reelStatus?.FailedHome == true)
                        {
                            localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Error);
                        }
                    }
                }
                else if (activeReel.IsSpinning && state != ReelControllerState.Tilted.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Spinning);
                }
                else if (activeReel.IsNudging && state != ReelControllerState.Tilted.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Nudging);
                }
                else if (state == ReelControllerState.Tilted.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Tilted);
                }
                else if (state == ReelControllerState.IdleUnknown.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReelController_IdleUnknown);
                }
                else
                {
                    var reelsStatus = ReelController.ReelsStatus;

                    if (reelsStatus.ContainsKey(reelId))
                    {
                        reelsStatus.TryGetValue(reelId, out var reelStatus);
                        if (reelStatus?.ReelStall == true)
                        {
                            localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Stalled);
                        }
                        else if (reelStatus?.ReelTampered == true)
                        {
                            localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Tampered);
                        }
                        else if (reelStatus?.LowVoltage == true)
                        {
                            localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LowVoltage);
                        }
                        else if (reelStatus?.FailedHome == true)
                        {
                            localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Error);
                        }
                        else if (reelStatus?.Connected == false)
                        {
                            localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disconnected);
                        }
                        else
                        {
                            localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Idle);
                        }
                    }
                }
            }
            else
            {
                if (state == ReelControllerState.Homing.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Homing);
                }
                else if (state == ReelControllerState.Disabled.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disabled);
                }
                else if (state == ReelControllerState.Disconnected.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disconnected);
                }
                else if (state == ReelControllerState.Inspecting.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Inspecting);
                }
                else if (state == ReelControllerState.Tilted.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Tilted);
                }
                else if (state == ReelControllerState.Uninitialized.ToString())
                {
                    localizedState = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Uninitialized);
                }
            }

            return localizedState;
        }

        private void HandleEvent(DisconnectedEvent @event)
        {
            UpdateScreen();
        }

        private void HandleEvent(ConnectedEvent @event)
        {
            UpdateScreen();
        }

        private void HandleEvent(ReelStoppedEvent @event)
        {
            Logger.Debug($"HandleEvent ReelStoppedEvent reelId={@event.ReelId}, step={@event.Step}, homing={@event.IsReelStoppedFromHoming}");

            if (@event.ReelId <= 0 || @event.ReelId > MaxSupportedReels)
            {
                return;
            }

            var activeReel = GetActiveReel(@event.ReelId);
            activeReel.Step = @event.Step.ToString();

            if (!activeReel.IsHoming)
            {
                IsHomed = false;
            }

            activeReel.IsHoming = false;
            activeReel.IsSpinning = false;
            activeReel.IsNudging = false;

            UpdateScreen();
        }

        private void HandleEvent(PropertyChangedEvent e)
        {
            if (e.PropertyName == nameof(ReelInfoItem.OffsetSteps))
            {
                SetOffSets();
            }
        }

        private void HandleEvent(HardwareFaultEvent @event)
        {
            SetHasFault();
        }

        private void HandleEvent(HardwareReelFaultEvent @event)
        {
            SetHasFault();
        }

        private void HandleEvent(HardwareDiagnosticTestStartedEvent @event)
        {
            if (@event.DeviceCategory == HardwareDiagnosticDeviceCategory.MechanicalReels)
            {
                IsHomed = false;
                UpdateScreen();
            }
        }

        private void HandleEvent(HardwareDiagnosticTestFinishedEvent @event)
        {
            if (@event.DeviceCategory == HardwareDiagnosticDeviceCategory.MechanicalReels)
            {
                IsHomed = true;
                UpdateScreen();
            }
        }

        private bool IsReelActive(int reel) => ReelInfo.Any(o => o.Id == reel);

        private void SetHasFault()
        {
            for (var i = 1; i <= MaxSupportedReels; ++i)
            {
                if (IsReelActive(i))
                {
                    var activeReel = GetActiveReel(i);
                    activeReel.IsHoming = false;
                    activeReel.IsSpinning = false;
                    activeReel.IsNudging = false;
                }
            }

            UpdateScreen();
        }

        private void SetReelControllerStatus()
        {
            var state = ReelController.LogicalState;

            if (state == ReelControllerState.IdleAtStops && IsHomed)
            {
                StatusText = "Homed";
            }
            else
            {
                StatusText = GetState(state.ToString());
            }

            var connectedReels = ReelController.ConnectedReels;
            ReelsEnabled = connectedReels.Count > 0;
        }

        private void SetReelsState()
        {
            if (ReelController == null)
            {
                return;
            }

            var reelStates = ReelController.ReelStates;

            for (var i = 1; i <= MaxSupportedReels; ++i)
            {
                if (IsReelActive(i))
                {
                    var activeReel = GetActiveReel(i);
                    activeReel.State = reelStates.ContainsKey(i)
                        ? GetState(reelStates.Where(x => x.Key == i).Select(x => x.Value).ToList()[0].ToString(), i)
                        : ReelLogicalState.Disconnected.ToString();

                    if (activeReel.State != Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Homing) &&
                        activeReel.State != Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Spinning) &&
                        activeReel.State != Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Nudging))
                    {
                        activeReel.IsHoming = false;
                        activeReel.IsSpinning = false;
                        activeReel.IsNudging = false;
                    }
                }
            }

            ReelTestViewModel.ReelInfo = ReelInfo;
        }

        private void SetOffSets()
        {
            var offsets = new int[MaxSupportedReels];

            for (var i = 1; i <= MaxSupportedReels; i++)
            {
                offsets[i - 1] = ReelInfo.FirstOrDefault(x => x.Id == i)?.OffsetSteps ?? 0;
            }

            ReelController.ReelOffsets = offsets;
        }
    }
}
