namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media;
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
    ///     The MechanicalReelsPageViewModel class
    /// </summary>
    [CLSCompliant(false)]
    public class MechanicalReelsPageViewModel : DeviceViewModel
    {
        private const int MaxSupportedReels = ReelConstants.MaxSupportedReels;
        private const int LightsOffTime = 250;
        private const int LightTestOnTime = 500;
        private const string SampleLightShowName = "SampleLightShow";
        private const string AllTag = "ALL";
        private const int RelmReelMaximumOffset = 3;

        private readonly IReelBrightnessCapabilities _brightnessCapabilities;
        private readonly IReelAnimationCapabilities _animationCapabilities;
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
        private bool _lightAnimationTestScreenHidden = true;
        private bool _lightTestScreenHidden = true;
        private bool _reelAnimationTestScreenHidden = true;
        private bool _reelTestScreenHidden = true;
        private bool _settingsScreenHidden;
        private bool _selfTestEnabled = true;
        private int _initialBrightness;
        private int _brightness;
        private int _maxReelOffset = 199;
        private bool _brightnessChanging;
        private IEdgeLightToken _offToken;

        /// <summary>
        ///     Instantiates a new instance of the MechanicalReelsPageViewModel class
        /// </summary>
        /// <param name="isWizard">Is this instance being used for the configuration wizard</param>
        public MechanicalReelsPageViewModel(bool isWizard) : base(DeviceType.ReelController, isWizard)
        {
            ShowLightTestCommand = new ActionCommand<object>(_ => ShowLightTest());
            ShowReelTestCommand = new ActionCommand<object>(_ => ShowReelTest());
            ShowSettingsCommand = new ActionCommand<object>(_ => ShowSettings());

            _edgeLightController = ServiceManager.GetInstance().GetService<IEdgeLightingController>();
            LightTestViewModel = new(ReelController, _edgeLightController, Inspection);
            LightAnimationTestViewModel = new(ReelController);
            ReelTestViewModel = new(ReelController, EventBus, MaxSupportedReels, ReelInfo, UpdateScreen, Inspection);
            ReelAnimationTestViewModel = new(ReelController, ReelInfo, UpdateScreen);

            SelfTestCommand = new ActionCommand<object>(_ => SelfTest(false));
            SelfTestClearCommand = new ActionCommand<object>(_ => SelfTest(true));
            ApplyBrightnessCommand = new ActionCommand<object>(_ => HandleApplyBrightness().FireAndForget());

            if (ReelController.HasCapability<IReelBrightnessCapabilities>())
            {
                _brightnessCapabilities = ReelController.GetCapability<IReelBrightnessCapabilities>();
            }

            if (ReelController.HasCapability<IReelAnimationCapabilities>())
            {
                _animationCapabilities = ReelController.GetCapability<IReelAnimationCapabilities>();
                MaxReelOffset = RelmReelMaximumOffset;
            }

            MinimumBrightness = 1;
            MaximumBrightness = 100;
        }
        
        /// <summary>
        ///     Gets the light animation test view model
        /// </summary>
        public MechanicalReelsLightAnimationTestViewModel LightAnimationTestViewModel { get; }

        /// <summary>
        ///     Gets the light animation test view model
        /// </summary>
        public MechanicalReelsAnimationTestViewModel ReelAnimationTestViewModel { get; }

        /// <summary>
        ///     Gets the light test view model
        /// </summary>
        public MechanicalReelsLightTestViewModel LightTestViewModel { get; }

        /// <summary>
        ///     Gets or sets a value indicating if the light animation test screen is hidden
        /// </summary>
        public bool LightAnimationTestScreenHidden
        {
            get => _lightAnimationTestScreenHidden;

            set
            {
                _lightAnimationTestScreenHidden = value;
                RaisePropertyChanged(nameof(LightAnimationTestScreenHidden));
                RaisePropertyChanged(nameof(LightTestButtonHidden));
                CancelLightTests();
            }
        }
        
        /// <summary>
        ///     Gets or sets a value indicating if the light test screen is hidden
        /// </summary>
        public bool LightTestScreenHidden
        {
            get => _lightTestScreenHidden;

            set
            {
                _lightTestScreenHidden = value;
                RaisePropertyChanged(nameof(LightTestScreenHidden));
                RaisePropertyChanged(nameof(LightTestButtonHidden));
                CancelLightTests();
            }
        }

        /// <summary>
        ///     Gets a value indicating if the light test button is hidden
        /// </summary>
        public bool LightTestButtonHidden => !LightTestScreenHidden || !LightAnimationTestScreenHidden;

        /// <summary>
        ///     Gets or sets the reel test view model
        /// </summary>
        public MechanicalReelsTestViewModel ReelTestViewModel { get; }

        /// <summary>
        ///     Gets or sets a value indicating if the reel animation test screen is hidden
        /// </summary>
        public bool ReelAnimationTestScreenHidden
        {
            get => _reelAnimationTestScreenHidden;

            set
            {
                _reelAnimationTestScreenHidden = value;
                RaisePropertyChanged(nameof(ReelAnimationTestScreenHidden));
                RaisePropertyChanged(nameof(ReelTestButtonHidden));
                CancelReelAnimationTests();
            }
        }
        
        /// <summary>
        ///     Gets or sets a value indicating if the reel test screen is hidden
        /// </summary>
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

        /// <summary>
        ///     Gets a value indicating if the reel test button is hidden
        /// </summary>
        public bool ReelTestButtonHidden => !ReelTestScreenHidden || !ReelAnimationTestScreenHidden;

        /// <summary>
        ///     Gets or sets a value indicating if the settings screen is hidden
        /// </summary>
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

        /// <summary>
        ///     Gets a value indicating if the settings button is hidden
        /// </summary>
        public bool SettingsButtonHidden => !SettingsScreenHidden;

        /// <summary>
        ///     Gets or sets the reel count
        /// </summary>
        public string ReelCount
        {
            get => _reelCount;

            set
            {
                _reelCount = value;
                RaisePropertyChanged(nameof(ReelCount));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating if the reels are enabled
        /// </summary>
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

        /// <summary>
        ///     Gets the info foreground color
        /// </summary>
        public SolidColorBrush InfoForeground => Brushes.White;

        /// <summary>
        ///     Gets the show light test command
        /// </summary>
        public ICommand ShowLightTestCommand { get; }
        
        /// <summary>
        ///     Gets the show reel test command
        /// </summary>
        public ICommand ShowReelTestCommand { get; }
        
        /// <summary>
        ///     Gets the show settings command
        /// </summary>
        public ICommand ShowSettingsCommand { get; }
        
        /// <summary>
        ///     Gets the self test command
        /// </summary>
        public ICommand SelfTestCommand { get; }
        
        /// <summary>
        ///     Gets the self test with RAM clear command
        /// </summary>
        public ICommand SelfTestClearCommand { get; }

        /// <summary>
        ///     Gets the apply brightness command
        /// </summary>
        public ICommand ApplyBrightnessCommand { get; }

        /// <summary>
        ///     Gets a value indicating if a brightness change is pending
        /// </summary>
        public bool BrightnessChangePending => InitialBrightness != Brightness;

        /// <summary>
        ///     Gets or sets a value indicating if the brightness is changing
        /// </summary>
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

        /// <summary>
        ///     Gets the minimum brightness
        /// </summary>
        public int MinimumBrightness { get; }

        /// <summary>
        ///     Gets the maximum brightness
        /// </summary>
        public int MaximumBrightness { get; }

        /// <inheritdoc />
        public override bool TestModeEnabledSupplementary => ReelController?.Connected ?? false;

        /// <summary>
        ///     Gets a value indicating if the test mode tooltip is disabled 
        /// </summary>
        public bool TestModeToolTipDisabled => TestModeEnabled;

        /// <summary>
        ///     Gets or sets the reel info collection
        /// </summary>
        public ObservableCollection<ReelInfoItem> ReelInfo { get; set; } = new();

        /// <summary>
        ///     Gets a value indicating if the reel controller has animation capabilities
        /// </summary>
        public bool IsAnimationController => _animationCapabilities != null;

        /// <summary>
        ///     Gets or sets the brightness
        /// </summary>
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

        /// <summary>
        ///     Gets or sets a value indicating if self test is enabled
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the initial brightness
        /// </summary>
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

        /// <summary>
        ///     The max number of steps to offset by
        /// </summary>
        public int MaxReelOffset
        {
            set
            {
                _maxReelOffset = value;
                RaisePropertyChanged(nameof(MaxReelOffset));
                RaisePropertyChanged(nameof(MinReelOffset));
            }
            get => _maxReelOffset;
        }

        /// <summary>
        ///     The minimum number of steps to offset by
        /// </summary>
        public int MinReelOffset => _maxReelOffset * -1;

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

        private void ShowLightTest()
        {
            if (ReelController is null)
            {
                return;
            }

            CancelReelAnimationTests();

            LightAnimationTestScreenHidden = !IsAnimationController;
            LightTestScreenHidden = IsAnimationController;

            ReelAnimationTestScreenHidden = true;
            ReelTestScreenHidden = true;
            SettingsScreenHidden = true;
            RaisePropertyChanged(nameof(IsAnimationController));
        }

        private void ShowReelTest()
        {
            if (ReelController is null)
            {
                return;
            }

            CancelLightTests();

            ReelAnimationTestScreenHidden = !IsAnimationController;
            ReelTestScreenHidden = IsAnimationController;
            
            LightAnimationTestScreenHidden = true;
            LightTestScreenHidden = true;
            SettingsScreenHidden = true;
            RaisePropertyChanged(nameof(IsAnimationController));
        }

        private void ShowSettings()
        {
            CancelLightTests();
            CancelReelAnimationTests();

            SettingsScreenHidden = false;
            
            LightAnimationTestScreenHidden = true;
            LightTestScreenHidden = true;
            ReelAnimationTestScreenHidden = true;
            ReelTestScreenHidden = true;
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

        /// <inheritdoc />
        protected override void OnLoaded()
        {
            base.OnLoaded();

            UpdateScreen();

            UpdateWarningMessage();

            Brightness = ReelControllerDefaultBrightness;
            InitialBrightness = Brightness;
        }
        
        /// <inheritdoc />
        protected override void OnUnloaded()
        {
            ClearPattern(ref _offToken);
            CancelLightTests();
            CancelReelAnimationTests();
            EventBus.UnsubscribeAll(this);
            base.OnUnloaded();
        }
        
        /// <inheritdoc />
        protected override void OnTestModeEnabledChanged()
        {
            RaisePropertyChanged(nameof(TestModeToolTipDisabled));

            LightTestScreenHidden = true;
            LightAnimationTestScreenHidden = true;
            ReelTestScreenHidden = true;
            ReelAnimationTestScreenHidden = true;
            SettingsScreenHidden = false;
        }
        
        /// <summary>
        ///     Sets the device information
        /// </summary>
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
                        var infoItem = new ReelInfoItem(
                            i,
                            true,
                            true,
                            ReelLogicalState.Disconnected.ToString(),
                            offsets.Length >= i ? offsets[i - 1] : 0);

                        ReelInfo.Add(infoItem);
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
            ReelAnimationTestViewModel.ReelInfo = ReelInfo;
        }
        
        /// <inheritdoc />
        protected override void StartEventHandler()
        {
            StartEventHandler(ReelController);
        }
        
        /// <inheritdoc />
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
        
        /// <inheritdoc />
        protected override void UpdateScreen()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    SetDeviceInformation();
                    SetReelControllerStatus();
                    SetReelsState();
                    ReelTestViewModel.UpdateScreen();
                    ReelAnimationTestViewModel.UpdateScreen();
                });
        }
        
        /// <inheritdoc />
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
                    await FlashLights();
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
            ReelAnimationTestViewModel.ReelInfo = ReelInfo;
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

        private void CancelLightTests()
        {
            LightTestViewModel?.CancelTest();
            LightAnimationTestViewModel?.CancelTest();
        }

        private void CancelReelAnimationTests()
        {
            ReelAnimationTestViewModel?.CancelTest();
        }

        private async Task FlashLights()
        {
            if (_animationCapabilities is null)
            {
                ClearPattern(ref _offToken);
                _offToken = _edgeLightController.AddEdgeLightRenderer(_solidBlackPattern);
                await Task.Delay(LightsOffTime);
                ClearPattern(ref _offToken);
            }
            else
            {
                var lightShow = new LightShowData
                {
                    Tag = AllTag,
                    Step = -1,
                    LoopCount = -1,
                    ReelIndex = -1,
                    AnimationName = SampleLightShowName
                };

                await _animationCapabilities.StopAllLightShows();
                await _animationCapabilities.PrepareAnimation(lightShow);
                await _animationCapabilities.PlayAnimations();
                await Task.Delay(LightTestOnTime);
                await _animationCapabilities.StopAllLightShows();
            }
        }
    }
}
