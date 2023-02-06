namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using Application.Settings;
    using Cabinet.Contracts;
    using ConfigWizard;
    using Contracts;
    using Contracts.HardwareDiagnostics;
    using Contracts.Input;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Kernel.Contracts;
    using Models;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using MVVM.Command;
    using Views;

    [CLSCompliant(false)]
    public class DisplaysPageViewModel : InspectionWizardViewModelBase
    {
        private static readonly TimeSpan IdentifyWindowDisplayTime = TimeSpan.FromSeconds(3);

        private readonly object _lock = new object();

        private CancellationTokenSource _cancellationTokenSource;
        private bool _testEnabled = true;
        private int _brightnessSliderValue = 100;
        private double _minimumBrightness;
        private double _maximumBrightness;
        private bool _touchScreenButtonsEnabled = true;
        private bool _calibrateTouchScreenVisible;
        private bool _testTouchScreenVisible = true;

        public DisplaysPageViewModel(bool isWizard) : base(isWizard)
        {
            EnterTouchScreenCommand = new ActionCommand<object>(OnEnterTouchScreenCommand);
            EnterIdentifyScreenCommand = new ActionCommand<object>(
                OnEnterIdentifyScreenCommand);
            EnterColorTestCommand = new ActionCommand<object>(
                OnEnterColorTestCommand);
            EnterCalibrateTouchScreenCommand = new ActionCommand<object>(
                OnEnterCalibrateTouchScreenCommand);
            CabinetService = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
            SerialTouchService = ServiceManager.GetInstance().GetService<ISerialTouchService>();
            SerialTouchCalibration = ServiceManager.GetInstance().GetService<ISerialTouchCalibration>();
        }

        public override bool CanCalibrateTouchScreens => false;

        public ObservableCollection<DisplayDetected> DisplaysDetected { get; } =
            new ObservableCollection<DisplayDetected>();

        public ICommand EnterTouchScreenCommand { get; }

        public ICommand EnterIdentifyScreenCommand { get; }

        public ICommand EnterColorTestCommand { get; }

        public ICommand EnterCalibrateTouchScreenCommand { get; }

        public ICabinetDetectionService CabinetService { get; }

        public ISerialTouchService SerialTouchService { get; }

        public ISerialTouchCalibration SerialTouchCalibration { get; }

        public bool TestsEnabled
        {
            get => _testEnabled && TestModeEnabled;
            set => SetProperty(ref _testEnabled, value, nameof(TestsEnabled));
        }

        public int BrightnessValue
        {
            get => _brightnessSliderValue;
            set
            {
                if (_brightnessSliderValue != value)
                {
                    SetProperty(ref _brightnessSliderValue, value, nameof(BrightnessValue));
                    PropertiesManager.SetProperty(ApplicationConstants.ScreenBrightness, value);
                    Task.Run(() => ChangeBrightness(value));
                }
            }
        }

        public double MinimumBrightness
        {
            get => _minimumBrightness;
            set
            {
                SetProperty(ref _minimumBrightness, value, nameof(MinimumBrightness));
                RaisePropertyChanged(nameof(MinimumBrightness));
            }
        }

        public double MaximumBrightness
        {
            get => _maximumBrightness;
            set
            {
                SetProperty(ref _maximumBrightness, value, nameof(MaximumBrightness));
                RaisePropertyChanged(nameof(MaximumBrightness));
            }
        }

        public bool IsCabinetThatAllowsChangingBrightness => PropertiesManager.GetValue(
            ApplicationConstants.CabinetBrightnessControlEnabled,
            false);

        public bool CalibrateTouchScreenVisible
        {
            get => _calibrateTouchScreenVisible;
            set => SetProperty(ref _calibrateTouchScreenVisible, value, nameof(CalibrateTouchScreenVisible));
        }

        public bool TestTouchScreenVisible
        {
            get => _testTouchScreenVisible;
            set => SetProperty(ref _testTouchScreenVisible, value, nameof(TestTouchScreenVisible));
        }

        public bool TouchScreenButtonsEnabled
        {
            get => TestsEnabled && _touchScreenButtonsEnabled;
            set => SetProperty(ref _touchScreenButtonsEnabled, value, nameof(TouchScreenButtonsEnabled));
        }

        protected override void OnLoaded()
        {
            EventBus.Subscribe<OperatorMenuEnteredEvent>(this, HandleOperatorMenuEntered);

            // get the monitor brightness setting and adjust the slider value to match
            if (IsCabinetThatAllowsChangingBrightness)
            {
                BrightnessValue = PropertiesManager.GetValue(
                    ApplicationConstants.ScreenBrightness,
                    PropertiesManager.GetValue(
                        ApplicationConstants.CabinetBrightnessControlDefault,
                        ApplicationConstants.DefaultBrightness));

                MinimumBrightness = PropertiesManager.GetValue(ApplicationConstants.CabinetBrightnessControlMin, 0);
                MaximumBrightness = PropertiesManager.GetValue(ApplicationConstants.CabinetBrightnessControlMax, 100);
            }

            UpdateStatusText();

            lock (_lock)
            {
                DisplaysDetected.Clear();

                DisplaysDetected.AddRange(
                    CabinetService.ExpectedDisplayDevices.OrderBy(d => d.PositionY).Select(
                        x => new DisplayDetected(
                            x,
                            GetMappedTouchDevice(x)
                            )));
                EventBus.Subscribe<DisplayConnectedEvent>(this, _ => RefreshDisplays());
                EventBus.Subscribe<DisplayDisconnectedEvent>(this, _ => RefreshDisplays());
                EventBus.Subscribe<TouchDisplayConnectedEvent>(this, _ => RefreshDisplays());
                EventBus.Subscribe<TouchDisplayDisconnectedEvent>(this, _ => RefreshDisplays());
                EventBus.Subscribe<DeviceConnectedEvent>(this, _ => RefreshDisplays());
                EventBus.Subscribe<DeviceDisconnectedEvent>(this, _ => RefreshDisplays());

                if (CabinetService.ExpectedDisplayDevicesWithSerialTouch != null)
                {
                    CalibrateTouchScreenVisible = TestTouchScreenVisible = CabinetService.ExpectedDisplayDevicesWithSerialTouch.Count() > 0;
                }
                else
                {
                    var touchDevicesAvailable = DisplaysDetected.Where(d => d.TouchDevice != null).ToList();
                    TestTouchScreenVisible = touchDevicesAvailable.Any();
                }

                RefreshDisplays();
            }

            var monitorInfo = "Monitor: ??"; // TODO
            var touchInfo = "Touch: " +
                            MachineSettingsUtilities.GetTouchScreenIdentificationWithoutVbd(Localizer.For(CultureFor.Operator));
            Inspection?.SetFirmwareVersion(string.Join(Environment.NewLine, monitorInfo, touchInfo));

            base.OnLoaded();
        }

        private ITouchDevice GetMappedTouchDevice(IDisplayDevice x)
        {
            ITouchDevice mappedTouchDevice = null;
            if (CabinetService.ExpectedSerialTouchDevices != null)
            {
                mappedTouchDevice = CabinetService.ExpectedSerialTouchDevices.FirstOrDefault(t => t.ProductId == x.TouchProductId && t.VendorId == x.TouchVendorId);
            }

            if (mappedTouchDevice == null && CabinetService.ExpectedTouchDevices != null)
            {
                mappedTouchDevice = CabinetService.ExpectedTouchDevices.FirstOrDefault(t => t.ProductId == x.TouchProductId && t.VendorId == x.TouchVendorId);
            }

            return mappedTouchDevice;
        }

        protected override void OnUnloaded()
        {
            _cancellationTokenSource?.Cancel();

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
            RaisePropertyChanged(nameof(TestsEnabled));
            RaisePropertyChanged(nameof(TouchScreenButtonsEnabled));
        }

        protected override void OnInputStatusChanged()
        {
            if (!IsCabinetThatAllowsChangingBrightness &&
                !InputEnabled &&
                (AccessRestriction == OperatorMenuAccessRestriction.ZeroCredits ||
                 AccessRestriction == OperatorMenuAccessRestriction.InGameRound))
            {
                var doorService = ServiceManager.GetInstance().GetService<IDoorService>();
                if (doorService != null)
                {
                    InputStatusText = doorService.GetDoorOpen((int)DoorLogicalId.Main)
                        ? string.Empty
                        : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenMainDoor);
                }
            }
        }

        protected override void UpdateStatusText()
        {
            if (IsCabinetThatAllowsChangingBrightness)
            {
                base.UpdateStatusText();
            }
            else
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent());
            }
        }

        private void RefreshDisplays()
        {
            var touchDisplaysConnected = DisplaysDetected.Where(d => !d.IsTouchDisconnected).ToList();
            TouchScreenButtonsEnabled = touchDisplaysConnected.Any();

            foreach (var displayDetected in DisplaysDetected)
            {
                displayDetected.Update();
            }
        }

        private void HandleOperatorMenuEntered(IEvent theEvent)
        {
            var operatorMenuEvent = (OperatorMenuEnteredEvent)theEvent;
            if (!operatorMenuEvent.IsTechnicianRole)
            {
                OnExitTouchScreenCommand();
            }
        }

        private async void OnEnterTouchScreenCommand(object obj)
        {
            if (!(obj is DependencyObject dependencyObject))
            {
                return;
            }

            var touchDisplays = DisplaysDetected.Where(d => d.TouchDevice != null).ToList();
            if (!touchDisplays.Any())
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            TestsEnabled = false;

            EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Displays));
            Inspection?.SetTestName("Touchscreen test");

            _cancellationTokenSource = new CancellationTokenSource();
            var testWindows = touchDisplays.Select(
                displayDetected =>
                {
                    var window =
                        new TouchScreenTestWindow
                        {
                            DisplayRole = displayDetected.DisplayDevice.Role,
                            Drawable = displayDetected.TouchDevice != null,
                            Owner = Window.GetWindow(dependencyObject)
                        };
                    window.Closed += ClosedHandler;
                    window.Show();
                    return window;
                }).ToList();

            await Task.Run(() => { WaitHandle.WaitAny(new[] { _cancellationTokenSource.Token.WaitHandle }); });
            testWindows.ForEach(
                w =>
                {
                    w.Close();
                    w.Closed -= ClosedHandler;
                });

            EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Displays));

            TestsEnabled = true;

            void ClosedHandler(object sender, EventArgs args)
            {
                _cancellationTokenSource?.Cancel();
            }
        }

        private void OnEnterCalibrateTouchScreenCommand(object obj)
        {
            TestsEnabled = false;
            EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Displays));
            Inspection?.SetTestName("Serial Touchscreen Calibration");
            InvokeSerialTouchCalibration();
        }

        private async void OnEnterColorTestCommand(object obj)
        {
            if (!(obj is DependencyObject dependencyObject))
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            TestsEnabled = false;

            EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Displays));
            Inspection?.SetTestName("Colors Test");

            _cancellationTokenSource = new CancellationTokenSource();
            var viewModel = new DisplayColorTestsViewModel(Inspection);
            var testWindows = DisplaysDetected.Select(
                displayDetected =>
                {
                    var window =
                        new DisplayColorTestWindow
                        {
                            DisplayRole = displayDetected.DisplayDevice.Role,
                            DisplayControls = displayDetected.DisplayDevice.Role == DisplayRole.Main,
                            Owner = Window.GetWindow(dependencyObject),
                            DataContext = viewModel
                        };
                    window.Closed += ClosedHandler;
                    window.Show();
                    return window;
                }).ToList();

            await Task.Run(() => { WaitHandle.WaitAny(new[] { _cancellationTokenSource.Token.WaitHandle }); });

            testWindows.ForEach(
                w =>
                {
                    w.Close();
                    w.Closed -= ClosedHandler;
                });

            EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Displays));

            TestsEnabled = true;

            void ClosedHandler(object sender, EventArgs args)
            {
                _cancellationTokenSource?.Cancel();
            }
        }

        private async void OnEnterIdentifyScreenCommand(object obj)
        {
            if (!(obj is DependencyObject dependencyObject))
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            ScreenIdentifyWindow screenIdentifyWindow = null;
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();

                TestsEnabled = false;

                EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Displays));
                Inspection?.SetTestName("Screen Identification");

                foreach (var display in DisplaysDetected)
                {
                    if (_cancellationTokenSource.IsCancellationRequested ||
                        display.DisplayDevice.Status != DeviceStatus.Connected)
                    {
                        continue;
                    }

                    Rectangle screenBounds = WindowToScreenMapper.GetScreenBounds(display.DisplayDevice);
                    var windowToScreenMapper = new WindowToScreenMapper(display.DisplayDevice.Role, true);
                    Rectangle visibleArea = windowToScreenMapper.GetVisibleArea();
                    // Translate the origin of the Visible Area for the global Screen coordinate space to
                    // local relative space. This is needed for ScreenIdentifyWindow as it takes a
                    // visible area rectangle that is relative to the origin of the Window. 
                    visibleArea.X -= screenBounds.X;
                    visibleArea.Y -= screenBounds.Y;

                    screenIdentifyWindow = new ScreenIdentifyWindow(display.DisplayNumber, true, screenBounds, visibleArea)
                    {
                        Owner = Window.GetWindow(dependencyObject)
                    };

                    screenIdentifyWindow.Show();
                    await Task.Delay(IdentifyWindowDisplayTime, _cancellationTokenSource.Token);
                    screenIdentifyWindow.Close();
                    screenIdentifyWindow = null;
                }
            }
            catch (OperationCanceledException)
            {
                //ignore
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
            finally
            {
                screenIdentifyWindow?.Close();
                _cancellationTokenSource = null;

                EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Displays));

                TestsEnabled = true;
            }
        }

        private void OnExitTouchScreenCommand()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void ChangeBrightness(int brightness, bool allMonitors = false)
        {
            var monitors = CabinetService.ExpectedDisplayDevices;
            foreach (var monitor in monitors)
            {
                monitor.Brightness = brightness;
                if (!allMonitors)
                {
                    break;
                }
            }
        }

        private void InvokeSerialTouchCalibration()
        {
            Logger.Debug($"InvokeSerialTouchCalibration - SerialTouchService.Initialized {SerialTouchService.Initialized}");

            if (SerialTouchService.Initialized)
            {
                if (SerialTouchCalibration.IsCalibrating)
                {
                    SerialTouchCalibration.CalibrateNextDevice();
                }
                else
                {
                    EventBus.Subscribe<SerialTouchCalibrationCompletedEvent>(this, OnSerialTouchCalibrationCompleted);
                    SerialTouchCalibration.BeginCalibration();
                }
            }
        }

        private void OnSerialTouchCalibrationCompleted(SerialTouchCalibrationCompletedEvent e)
        {
            if (SerialTouchService.PendingCalibration)
            {
                Logger.Info("Requesting reboot with pending serial touch calibration.");
                EventBus.Publish(new ExitRequestedEvent(ExitAction.Reboot));
                return;
            }

            InvokeSerialTouchCalibration();
            Logger.Debug($"OnSerialTouchCalibrationCompleted Success {e.Success} Status {e.Status}");

            var success = e.Success;
            if (success)
            {
                EventBus.Unsubscribe<SerialTouchCalibrationCompletedEvent>(this);
                EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Displays));
                TestsEnabled = true;
            }
        }
    }
}