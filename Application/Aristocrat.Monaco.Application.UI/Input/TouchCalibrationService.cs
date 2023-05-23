namespace Aristocrat.Monaco.Application.UI.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Input;
    using Contracts.Input;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Display;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common;
    using MVVM;

    public sealed class TouchCalibrationService : ITouchCalibration, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly ICabinetDetectionService _cabinetDetection;
        private readonly List<TouchCalibrationWindow> _calibrationWindows = new();

        private TouchCalibrationWindow _activeWindow;
        private TouchCalibrationOverlayWindow _overlay;

        public TouchCalibrationService() : this(
            ServiceManager.GetInstance().GetService<IEventBus>(),
            ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TouchCalibrationService" /> class.
        /// </summary>
        /// <param name="eventBus">The event bus.</param>
        /// <param name="cabinetDetection">The cabinet detection.</param>
        [CLSCompliant(false)]
        public TouchCalibrationService(
            IEventBus eventBus,
            ICabinetDetectionService cabinetDetection)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _cabinetDetection = cabinetDetection ?? throw new ArgumentNullException(nameof(cabinetDetection));

            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, HandleEvent);
            _eventBus.Subscribe<DisplayDisconnectedEvent>(this, HandleEvent);
        }

        public bool IsCalibrating { get; private set; }

        public string Name => nameof(TouchCalibrationService);

        public ICollection<Type> ServiceTypes => new[] { typeof(ITouchCalibration) };

        public void Initialize()
        {
        }

        public bool BeginCalibration()
        {
            if (IsCalibrating)
            {
                Logger.Error($"{nameof(BeginCalibration)}() called while calibration is already in progress.");
                return false;
            }

            Logger.Info("BeginCalibration - Started touch screen calibration");

            IsCalibrating = true;

            InitializeCalibration();

            return true;
        }

        public void CalibrateNextDevice()
        {
            if (!IsCalibrating)
            {
                throw new Exception($"{nameof(CalibrateNextDevice)}() called before calibration has begun.");
            }

            Logger.Debug("Progressing on to calibrate next touch Device.");

            MvvmHelper.ExecuteOnUI(() => _activeWindow = _activeWindow?.NextCalibrationTest());
        }

        public void AbortCalibration(string displayMessage = "")
        {
            if (!IsCalibrating)
            {
                return;
            }

            Logger.Info("Aborting touch screen calibration...");

            FinalizeCalibration(true, "Touch calibration aborted.", displayMessage);
        }

        private void InitializeCalibration()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    TouchCalibrationWindow prevControl = null;

                    List<long> serialTouchDisplayIds = new List<long>();
                    if (_cabinetDetection.ExpectedDisplayDevicesWithSerialTouch != null)
                    {
                        foreach (var display in _cabinetDetection.ExpectedDisplayDevicesWithSerialTouch)
                        {
                            serialTouchDisplayIds.Add(display.DisplayId);
                        }
                    }

                    // Create touch calibration control for each display
                    foreach (var display in _cabinetDetection.ExpectedDisplayDevices)
                    {
                        if (!serialTouchDisplayIds.Contains(display.DisplayId))
                        {
                            Logger.Debug($"InitializeCalibration - Adding display {display.DeviceName} {display.DisplayId} to touch calibration test.");
                            display.TouchProductId = display.TouchVendorId = 0;
                            var calibrationWindow = new TouchCalibrationWindow { Monitor = display };

                            if (prevControl != null)
                            {
                                prevControl.NextDevice = calibrationWindow;
                            }

                            // Set active window to first calibration window
                            if (!_calibrationWindows.Any())
                            {
                                _activeWindow = calibrationWindow;
                            }

                            // Register to handle calibration completion
                            calibrationWindow.CalibrationComplete += OnCalibrationComplete;

                            calibrationWindow.Show();
                            prevControl = calibrationWindow;
                            _calibrationWindows.Add(calibrationWindow);
                        }
                        else
                        {
                           Logger.Debug($"InitializeCalibration - Skipping display {display.DeviceName} {display.DisplayId} mapped for serial touch");
                        }
                    }

                    // Do we have any calibration windows (IE. only serial touch)?
                    if (!_calibrationWindows.Any())
                    {
                        // No, finalize and return
                        Logger.Debug($"InitializeCalibration - No calibration windows, finalizing...");
                        FinalizeCalibration(false, null);
                        return;
                    }

                    // The overlay must be the last item so we capture touch correctly
                    if (_overlay != null)
                    {
                        RemoveOverlay();
                    }

                    _overlay = new TouchCalibrationOverlayWindow(_cabinetDetection);
                    _overlay.PreviewTouchDown += OnPreviewTouchDown;
                    _overlay.PreviewKeyDown += OnPreviewKeyDown;
                    _overlay.Show();

                    _activeWindow.BeginCalibrationTest();
                });
        }

        private void FinalizeCalibration(bool aborted, string message = "", string displayMessage = "")
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    Logger.Debug($"FinalizeCalibration - Finalizing calibration session - aborted {aborted}");

                    foreach (var window in _calibrationWindows)
                    {
                        window.CalibrationComplete -= OnCalibrationComplete;
                        window.Close();
                    }

                    _calibrationWindows.Clear();

                    _activeWindow = null;
                    RemoveOverlay();

                    if (!aborted)
                    {
                        if (HasErrors(out var error))
                        {
                            message = string.IsNullOrWhiteSpace(message) ? error : message + " " + error;
                            aborted = true;
                        }
                        else
                        {
                            _cabinetDetection.MapTouchscreens(true);
                        }
                    }

                    IsCalibrating = false;

                    _eventBus.Publish(new TouchCalibrationCompletedEvent(!aborted, message, displayMessage));
                });
        }

        private void RemoveOverlay()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (_overlay == null)
                    {
                        return;
                    }

                    _overlay.PreviewTouchDown -= OnPreviewTouchDown;
                    _overlay.PreviewKeyDown -= OnPreviewKeyDown;
                    _overlay.Close();
                    _overlay = null;
                });
        }

        private bool HasErrors(out string error)
        {
            var touchscreenCount = _cabinetDetection.ExpectedTouchDevices.Count();
            if (touchscreenCount > 0 && _cabinetDetection.ExpectedDisplayDevicesWithSerialTouch != null)
            {
                touchscreenCount -= _cabinetDetection.ExpectedDisplayDevicesWithSerialTouch.Count();
            }

            Logger.Info($"{nameof(ICabinetDetectionService)} reports {touchscreenCount} touch screens.");

            var mappedDevices = _cabinetDetection.ExpectedDisplayDevices
                .Where(m => m.TouchVendorId != 0 && m.TouchProductId != 0)
                .GroupBy(m => (m.TouchVendorId, m.TouchProductId)).ToList();
            var mappedTwice = mappedDevices.Any(x => x.Count() > 1);

            if (mappedTwice)
            {
                error = $"Calibration Error: Calibrated Count: {mappedDevices.Count}, Same touch device mapped twice.";
                return true;
            }

            if (touchscreenCount > mappedDevices.Count)
            {
                error = "Calibration Error: Some touchscreens were not included in the calibration";
                return true;
            }

            error = null;
            return false;
        }

        private (int? vendorId, int? productId) GetDeviceVidPid(TouchDevice device)
        {
            if (device is TouchDeviceEmulator emulator)
            {
                var deviceData = new WindowsServices.RID_DEVICE_INFO();
                var size = Convert.ToUInt32(Marshal.SizeOf<WindowsServices.RID_DEVICE_INFO>());
                WindowsServices.GetTouchDeviceInfo(emulator.HSource, ref deviceData, ref size);
                return deviceData.dwType == (int)WindowsServices.DeviceType.HID
                    ? (deviceData.hid.dwVendorId, deviceData.hid.dwProductId)
                    : ((int?)null, (int?)null);
            }

            var pointer = _cabinetDetection.TouchDeviceByCursorId(device.Id);
            return (pointer?.VendorId, pointer?.ProductId);
        }

        private void OnPreviewTouchDown(object sender, TouchEventArgs args)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (_activeWindow != null)
                    {
                        var (vendorId, productId) = GetDeviceVidPid(args.TouchDevice);
                        var monitor = _activeWindow.Monitor;

                        Logger.Debug(
                            $"Touched Monitor: DeviceName={monitor.DeviceName}, DisplayId={monitor.DisplayId}, ConnectorId={monitor.ConnectorId}");
                        if (productId.HasValue && vendorId.HasValue)
                        {
                            Logger.Debug(
                                $"Touch Device: Id={args.TouchDevice.Id}, VID={vendorId:X4}, PID={productId:X4}");
                            monitor.TouchVendorId = vendorId.Value;
                            monitor.TouchProductId = productId.Value;
                        }
                    }

                    CalibrateNextDevice();
                });
        }

        private void OnPreviewKeyDown(object o, KeyEventArgs args)
        {
            Logger.Debug($"TouchCalibration Key Down({args.Key}) for monitor {_activeWindow?.Monitor.DeviceName}");

            // prevents KeyDown event from propagating to underlying windows
            args.Handled = true;

            MvvmHelper.ExecuteOnUI(CalibrateNextDevice);
        }

        private void OnCalibrationComplete(object o, EventArgs args)
        {
            if (!IsCalibrating)
            {
                return;
            }

            FinalizeCalibration(false, null);
        }

        private void HandleEvent(OperatorMenuExitedEvent evt)
        {
            Logger.Info("Operator Menu exited -- Touch calibration cancelled.");

            if (!IsCalibrating)
            {
                return;
            }

            FinalizeCalibration(true, "Touch calibration aborted via exit Operator Menu.");
        }

        private void HandleEvent(DisplayDisconnectedEvent evt)
        {
            if (_overlay != null)
            {
                AbortCalibration(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TouchCalibrationDisconnectError));
            }
        }
    }
}