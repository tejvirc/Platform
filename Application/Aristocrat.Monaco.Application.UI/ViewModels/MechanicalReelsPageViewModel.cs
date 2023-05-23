namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ImplementationCapabilities;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Hardware.Contracts.Reel.ControlData;
    using Hardware.Contracts.Reel.Events;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Models;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using MVVM;
    using MVVM.Command;
    using Color = System.Drawing.Color;

    /// <summary>
    ///     View model for mechanical reels
    /// </summary>
    [CLSCompliant(false)]
    public class MechanicalReelsPageViewModel : DeviceViewModel
    {
        private const int MaxSupportedReels = ReelConstants.MaxSupportedReels;
        private const int LightsOffTime = 250;
        
        private readonly IReelBrightnessCapabilities _brightnessCapabilities;
        private readonly IReelAnimationCapabilities _reelAnimationCapabilities;
        private readonly IEdgeLightingController _edgeLightController;
        private readonly PatternParameters _solidBlackPattern = new SolidColorPatternParameters
        {
            Color = Color.Black,
            Priority = StripPriority.PlatformTest,
            Strips = new[]
            {
                (int)StripIDs.StepperReel1,
                (int)StripIDs.StepperReel2,
                (int)StripIDs.StepperReel3,
                (int)StripIDs.StepperReel4,
                (int)StripIDs.StepperReel5
            }
        };

        private string _reelCount;
        private bool _reelsEnabled;
        private bool _lightTestScreenHidden = true;
        private bool _reelTestScreenHidden = true;
        private bool _settingsScreenHidden;
        private bool _selfTestEnabled = true;
        private int _initialBrightness;
        private int _brightness;
        private bool _brightnessChanging;
        private IEdgeLightToken _offToken;

        public MechanicalReelsPageViewModel(bool isWizard) : base(DeviceType.ReelController, isWizard)
        {
            ShowLightTestCommand = new ActionCommand<object>(_ =>
            {
                LightTestScreenHidden = false;
                ReelTestScreenHidden = true;
                SettingsScreenHidden = true;
            });
            ShowReelTestCommand = new ActionCommand<object>(_ =>
            {
                LightTestScreenHidden = true;
                ReelTestScreenHidden = false;
                SettingsScreenHidden = true;
                LightTestViewModel?.CancelTest();
            });
            ShowSettingsCommand = new ActionCommand<object>(_ =>
            {
                LightTestScreenHidden = true;
                ReelTestScreenHidden = true;
                SettingsScreenHidden = false;
                LightTestViewModel?.CancelTest();
            });

            _edgeLightController = ServiceManager.GetInstance().GetService<IEdgeLightingController>();
            LightTestViewModel = new(ReelController, _edgeLightController, Inspection);
            ReelTestViewModel = new(ReelController, EventBus, MaxSupportedReels, ReelInfo, UpdateScreen, Inspection);

            SelfTestCommand = new ActionCommand<object>(_ => SelfTest(false));
            SelfTestClearCommand = new ActionCommand<object>(_ => SelfTest(true));
            ApplyBrightnessCommand = new ActionCommand<object>(_ => HandleApplyBrightness().FireAndForget());

            if (ReelController.HasCapability<IReelBrightnessCapabilities>())
            {
                _brightnessCapabilities = ReelController.GetCapability<IReelBrightnessCapabilities>();
            }

            if (ReelController.HasCapability<IReelAnimationCapabilities>())
            {
                _reelAnimationCapabilities = ReelController.GetCapability<IReelAnimationCapabilities>();
            }

            MinimumBrightness = 1;
            MaximumBrightness = 100;

            LightTestButtonClicked = new ActionCommand<object>(_ =>
            {
                Task.Run(() => TestRemoveAllAnimations().ConfigureAwait(false));
            });
        }

        public MechanicalReelsLightTestViewModel LightTestViewModel { get; }

        public MechanicalReelsTestViewModel ReelTestViewModel { get; }

        public bool ReelTestScreenHidden
        {
            get => _reelTestScreenHidden;

            set
            {
                _reelTestScreenHidden = value;
                RaisePropertyChanged(nameof(ReelTestScreenHidden));
                RaisePropertyChanged(nameof(ReelTestButtonHidden));
            }
        }

        public bool ReelTestButtonHidden => !ReelTestScreenHidden;

        public bool LightTestScreenHidden
        {
            get => _lightTestScreenHidden;

            set
            {
                _lightTestScreenHidden = value;
                RaisePropertyChanged(nameof(LightTestScreenHidden));
                RaisePropertyChanged(nameof(LightTestButtonHidden));

                LightTestViewModel.CancelTest();
            }
        }

        public bool LightTestButtonHidden => !LightTestScreenHidden;

        public bool SettingsScreenHidden
        {
            get => _settingsScreenHidden;

            set
            {
                _settingsScreenHidden = value;
                RaisePropertyChanged(nameof(SettingsScreenHidden));
                RaisePropertyChanged(nameof(SettingsButtonHidden));
            }
        }

        public bool SettingsButtonHidden => !SettingsScreenHidden;

        public string ReelCount
        {
            get => _reelCount;

            set
            {
                _reelCount = value;
                RaisePropertyChanged(nameof(ReelCount));
                RaisePropertyChanged(nameof(ReelCountForeground));
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
                    RaisePropertyChanged(nameof(ReelsEnabled));
                }
            }
        }

        public SolidColorBrush ReelCountForeground => Brushes.White;

        public ICommand ShowLightTestCommand { get; }

        public ICommand ShowReelTestCommand { get; }

        public ICommand ShowSettingsCommand { get; }

        public ICommand SelfTestCommand { get; }

        public ICommand SelfTestClearCommand { get; }

        public ICommand ApplyBrightnessCommand { get; }

        public ICommand LightTestButtonClicked { get; }

        public bool BrightnessChangePending => InitialBrightness != Brightness;

        public bool BrightnessChanging
        {
            get => _brightnessChanging;

            set
            {
                if (_brightnessChanging != value)
                {
                    _brightnessChanging = value;
                    RaisePropertyChanged(nameof(BrightnessChanging));
                }
            }
        }

        public int MinimumBrightness { get; }

        public int MaximumBrightness { get; }

        public override bool TestModeEnabledSupplementary => ReelController?.Connected ?? false;

        public bool TestModeToolTipDisabled => TestModeEnabled;

        public ObservableCollection<ReelInfoItem> ReelInfo { get; set; } = new();

        public int Brightness
        {
            get => _brightness;

            set
            {
                if (_brightness != value)
                {
                    _brightness = value;
                    RaisePropertyChanged(nameof(Brightness));
                    RaisePropertyChanged(nameof(BrightnessChangePending));
                }
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
                RaisePropertyChanged(nameof(SelfTestEnabled));
            }
        }

        public int InitialBrightness
        {
            get => _initialBrightness;

            set
            {
                if (_initialBrightness != value)
                {
                    _initialBrightness = value;
                    RaisePropertyChanged(nameof(BrightnessChangePending));
                }
            }
        }

        private int ReelControllerDefaultBrightness
        {
            get => _brightnessCapabilities?.DefaultReelBrightness ?? MaximumBrightness;

            set
            {
                if (_brightnessCapabilities is not null &&
                    _brightnessCapabilities.DefaultReelBrightness != value)
                {
                    _brightnessCapabilities.DefaultReelBrightness = value;
                }
            }
        }

        private void ClearPattern(ref IEdgeLightToken token)
        {
            if (token == null)
            {
                return;
            }

            _edgeLightController.RemoveEdgeLightRenderer(token);
            token = null;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            UpdateScreen();

            UpdateWarningMessage();

            Brightness = ReelControllerDefaultBrightness;
            InitialBrightness = Brightness;
        }

        protected override void OnUnloaded()
        {
            ClearPattern(ref _offToken);
            LightTestViewModel.CancelTest();
            EventBus.UnsubscribeAll(this);
            base.OnUnloaded();
        }

        protected override void OnTestModeEnabledChanged()
        {
            RaisePropertyChanged(nameof(TestModeToolTipDisabled));

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
            MvvmHelper.ExecuteOnUI(
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

            RaisePropertyChanged(nameof(TestModeToolTipDisabled));
        }

        private void OnSelfTestCmd(object obj)
        {
            SelfTest(false);
        }

        private void OnSelfTestClearCmd(object obj)
        {
            SelfTest(true);
        }

        private async void SelfTest(bool clearNvm)
        {
            SelfTestEnabled = false;
            Inspection?.SetTestName($"Self test {(clearNvm ? " clear NVM" : "")}");

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

        private async Task HandleApplyBrightness()
        {
            BrightnessChanging = true;

            InitialBrightness = Brightness;
            ReelControllerDefaultBrightness = Brightness;

            if (ReelControllerDefaultBrightness == Brightness)
            {
                if (_brightnessCapabilities is not null)
                {
                    await _brightnessCapabilities.SetBrightness(Brightness);
                
                    ClearPattern(ref _offToken);
                    _offToken = _edgeLightController.AddEdgeLightRenderer(_solidBlackPattern);
                    await Task.Delay(LightsOffTime);
                    ClearPattern(ref _offToken);
                }
            }
            else
            {
                Brightness = ReelControllerDefaultBrightness;
                InitialBrightness = Brightness;
            }

            BrightnessChanging = false;
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
            Inspection?.SetTestName($"Controller fault {@event.Fault}");
            Inspection?.ReportTestFailure();
        }

        private void HandleEvent(HardwareReelFaultEvent @event)
        {
            SetHasFault();
            Inspection?.SetTestName($"Reel fault {@event.Fault}");
            Inspection?.ReportTestFailure();
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

        private async Task TestRemoveAllAnimations()
        {
            if (_reelAnimationCapabilities is null)
            {
                return;
            }

            var cts = new CancellationTokenSource();
            string currentDir = Directory.GetCurrentDirectory();
            await _reelAnimationCapabilities.RemoveAllControllerAnimations(cts.Token);
            await _reelAnimationCapabilities.LoadControllerAnimationFile(new Hardware.Contracts.Reel.ControlData.AnimationData($@"{currentDir}\Reel\LightShows\AllWhite5Reels.lightshow", AnimationType.LightShow), default);
        }
    }
}
