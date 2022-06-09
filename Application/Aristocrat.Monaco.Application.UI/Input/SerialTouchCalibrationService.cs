namespace Aristocrat.Monaco.Application.UI.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using Aristocrat.Monaco.Localization.Properties;
    using Contracts.Input;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using MVVM;

    public class SerialTouchCalibrationService : ISerialTouchCalibration, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int CalibrationErrorTimeoutSeconds = 10;
        private const int MilliSecondsPerSecond = 1000;

        private readonly IEventBus _eventBus;
        private readonly ICabinetDetectionService _cabinetDetection;
        private readonly object _lockObject = new object();
        private readonly List<SerialTouchCalibrationWindow> _calibrationWindows = new List<SerialTouchCalibrationWindow>();
        private SerialTouchCalibrationWindow _activeWindow;

        private Timer _calibrationErrorTimer;
        private int _timerElapsedSeconds;

        private bool _disposed;

        public SerialTouchCalibrationService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerialTouchCalibrationService" /> class.
        /// </summary>
        /// <param name="eventBus">The event bus.</param>
        /// <param name="cabinetDetection">The cabinet detection.</param>
        [CLSCompliant(false)]
        public SerialTouchCalibrationService(
            IEventBus eventBus,
            ICabinetDetectionService cabinetDetection)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _cabinetDetection = cabinetDetection ?? throw new ArgumentNullException(nameof(cabinetDetection));

            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, OnOperatorMenuExit);
            _eventBus.Subscribe<SerialTouchCalibrationStatusEvent>(this, OnSerialTouchCalibrationStatusEvent);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool CalibrationError { get; private set; }

        public bool IsCalibrating { get; private set; }

        public string Name => nameof(SerialTouchCalibrationService);

        public ICollection<Type> ServiceTypes => new[] { typeof(ISerialTouchCalibration) };

        public void AbortCalibration()
        {
            if (!IsCalibrating)
            {
                return;
            }

            Logger.Info("Aborting serial touch screen calibration...");

            FinalizeCalibration(true, "Serial touch calibration aborted.");
        }

        public bool BeginCalibration()
        {
            if (IsCalibrating)
            {
                Logger.Error($"{nameof(BeginCalibration)}() called while calibration is already in progress.");
                return false;
            }

            Logger.Info("Started serial touch screen calibration");

            IsCalibrating = true;
            CalibrationError = false;

            InitializeCalibration();

            return true;
        }

        public void CalibrateNextDevice()
        {
            if (!IsCalibrating)
            {
                throw new Exception($"{nameof(CalibrateNextDevice)}() called before calibration has begun.");
            }

            Logger.Debug("CalibrateNextDevice - Progressing on to calibrate next serial touch device...");

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _activeWindow = _activeWindow?.NextCalibrationTest();
                });
        }

        public void Initialize()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                StopTimer();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void InitializeCalibration()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    SerialTouchCalibrationWindow prevControl = null;

                    // Create SerialTouchCalibration control for each expected display with serial touch
                    foreach (var display in _cabinetDetection.ExpectedDisplayDevicesWithSerialTouch)
                    {
                        Logger.Debug($"InitializeCalibration - Adding display {display.DeviceName} to the serial touch calibration test.");
                        display.TouchProductId = display.TouchVendorId = 0;
                        var calibrationWindow = new SerialTouchCalibrationWindow { Monitor = display };

                        if (prevControl != null)
                        {
                            prevControl.NextDevice = calibrationWindow;
                        }

                        // Skip Display
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

                    _activeWindow.BeginCalibrationTest();
                });
        }

        private void FinalizeCalibration(bool aborted, string message = "")
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    Logger.Debug($"FinalizeCalibration - Aborted {aborted} - IsCalibrating {IsCalibrating} - CalibrationError {CalibrationError}");

                    foreach (var window in _calibrationWindows)
                    {
                        window.CalibrationComplete -= OnCalibrationComplete;
                        window.Close();
                    }

                    _calibrationWindows.Clear();

                    _activeWindow = null;

                    IsCalibrating = false;
                    CalibrationError = false;

                    _eventBus.Publish(new SerialTouchCalibrationCompletedEvent(true, message));
                });
        }

        /// <summary>Starts the calibration error timer for given timeout value.</summary>
        /// <param name="timeout">Timeout value in milliseconds.</param>
        private void StartTimer(int timeout)
        {
            _timerElapsedSeconds = 0;
            _calibrationErrorTimer = new Timer();
            _calibrationErrorTimer.Elapsed += OnCalibrationErrorTimeout;
            _calibrationErrorTimer.Interval = timeout;
            _calibrationErrorTimer.AutoReset = true;
            _calibrationErrorTimer.Start();
        }

        private void OnCalibrationErrorTimeout(object sender, ElapsedEventArgs e)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _timerElapsedSeconds++;
                    if (_timerElapsedSeconds >= CalibrationErrorTimeoutSeconds)
                    {
                        _activeWindow?.UpdateError(string.Empty);
                        StopTimer();
                        _eventBus.Publish(new ExitRequestedEvent(ExitAction.Reboot));                          
                    }
                    else
                    { 
                        var error = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SerialCalibrationError), CalibrationErrorTimeoutSeconds - _timerElapsedSeconds);
                        _activeWindow?.UpdateError(error);
                    }
                });
        }

        private void StopTimer()
        {
            lock (_lockObject)
            {
                if (_calibrationErrorTimer != null)
                {
                    _calibrationErrorTimer.Stop();
                    _calibrationErrorTimer.Elapsed -= OnCalibrationErrorTimeout;
                    _calibrationErrorTimer.Dispose();
                    _calibrationErrorTimer = null;
                }
            }
        }

        private void OnCalibrationComplete(object o, EventArgs args)
        {
            if (!IsCalibrating)
            {
                return;
            }

            FinalizeCalibration(false, null);
        }

        private void OnOperatorMenuExit(OperatorMenuExitedEvent evt)
        {
            Logger.Info("Operator Menu exited -- Serial touch calibration aborted.");
            FinalizeCalibration(true, "Serial touch calibration aborted via exit Operator Menu.");
        }

        private void OnSerialTouchCalibrationStatusEvent(SerialTouchCalibrationStatusEvent e)
        {
            if (IsCalibrating)
            {
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        _activeWindow?.UpdateCalibration(e);
                        if (!string.IsNullOrEmpty(e.ResourceKey))
                        {
                            if (!string.IsNullOrEmpty(e.Error))
                            {
                                if (e.ResourceKey == ResourceKeys.TouchCalibrateModel)
                                {
                                    var message = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AftLockMessage)); // Please Wait...
                                    _activeWindow?.UpdateError(message);
                                }
                                else
                                {
                                    CalibrationError = true;
                                    var error = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SerialCalibrationError), CalibrationErrorTimeoutSeconds);
                                    _activeWindow?.UpdateError(error);
                                    StartTimer(MilliSecondsPerSecond);
                                }
                            }
                            else
                            {
                                _activeWindow?.UpdateError(string.Empty);
                            }
                        }
                    });
            }
        }
    }
}