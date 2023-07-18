namespace Aristocrat.Monaco.Application.UI.Input
{
    using System;
    using System.Collections.Generic;
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

    public class SerialTouchCalibrationService : ISerialTouchCalibration, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private const int CalibrationErrorTimeoutSeconds = 10;
        private const int MilliSecondsPerSecond = 1000;

        private readonly IEventBus _eventBus;
        private readonly ICabinetDetectionService _cabinetDetection;
        private readonly ISerialTouchService _serialTouchService;
        private readonly object _lockObject = new();
        private readonly LinkedList<(SerialTouchCalibrationViewModel ViewModel, SerialTouchCalibrationWindow Window)> _touchCalibrations = new();
        private LinkedListNode<(SerialTouchCalibrationViewModel ViewModel, SerialTouchCalibrationWindow Window)> _activeCalibration;

        private Timer _calibrationErrorTimer;
        private int _timerElapsedSeconds;

        private bool _disposed;

        public SerialTouchCalibrationService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>(),
                ServiceManager.GetInstance().GetService<ISerialTouchService>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerialTouchCalibrationService" /> class.
        /// </summary>
        /// <param name="eventBus">The event bus.</param>
        /// <param name="cabinetDetection">The cabinet detection.</param>
        /// <param name="serialTouchService">The serial touch service</param>
        [CLSCompliant(false)]
        public SerialTouchCalibrationService(
            IEventBus eventBus,
            ICabinetDetectionService cabinetDetection,
            ISerialTouchService serialTouchService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _cabinetDetection = cabinetDetection ?? throw new ArgumentNullException(nameof(cabinetDetection));
            _serialTouchService = serialTouchService ?? throw new ArgumentNullException(nameof(serialTouchService));

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
                Logger.Error("Trying to start calibration when it is already started");
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
                throw new Exception("Calibration is not active");
            }

            Logger.Debug("CalibrateNextDevice - Progressing on to calibrate next serial touch device...");
            MvvmHelper.ExecuteOnUI(() =>
            {
                _activeCalibration?.Value.ViewModel.CompleteCalibrationTest();
                _activeCalibration = _activeCalibration?.Next;
                HandleCalibration();
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
                { // Create SerialTouchCalibration control for each expected display with serial touch
                    foreach (var display in _cabinetDetection.ExpectedDisplayDevicesWithSerialTouch)
                    {
                        Logger.Debug($"InitializeCalibration - Adding display {display.DeviceName} to the serial touch calibration test.");
                        display.TouchProductId = display.TouchVendorId = 0;
                        var calibrationViewModel = new SerialTouchCalibrationViewModel(display, _serialTouchService.Model);
                        var calibrationWindow = new SerialTouchCalibrationWindow(calibrationViewModel)
                        {
                            Monitor = display
                        };

                        calibrationWindow.Show();
                        _touchCalibrations.AddLast((calibrationViewModel, calibrationWindow));
                    }

                    _activeCalibration = _touchCalibrations.First;
                    HandleCalibration();
                });
        }

        private void HandleCalibration()
        {
            if (_activeCalibration is null)
            {
                OnCalibrationComplete();
            }
            else
            {
                _activeCalibration.Value.ViewModel.BeginCalibrationTest();
                _serialTouchService.StartCalibration();
            }
        }

        private void FinalizeCalibration(bool aborted, string message = "")
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    Logger.Debug($"FinalizeCalibration - Aborted {aborted} - IsCalibrating {IsCalibrating} - CalibrationError {CalibrationError}");

                    foreach (var (_, window) in _touchCalibrations)
                    {
                        window.Close();
                    }

                    _touchCalibrations.Clear();
                    _activeCalibration = null;

                    IsCalibrating = false;
                    CalibrationError = false;

                    if (aborted)
                    {
                        _serialTouchService.CancelCalibration();
                    }

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
                        _activeCalibration?.Value.ViewModel.UpdateError(string.Empty);
                        StopTimer();
                        _eventBus.Publish(new ExitRequestedEvent(ExitAction.Reboot));
                    }
                    else
                    {
                        var error = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SerialCalibrationError), CalibrationErrorTimeoutSeconds - _timerElapsedSeconds);
                        _activeCalibration?.Value.ViewModel.UpdateError(error);
                    }
                });
        }

        private void StopTimer()
        {
            lock (_lockObject)
            {
                if (_calibrationErrorTimer == null)
                {
                    return;
                }

                _calibrationErrorTimer.Stop();
                _calibrationErrorTimer.Elapsed -= OnCalibrationErrorTimeout;
                _calibrationErrorTimer.Dispose();
                _calibrationErrorTimer = null;
            }
        }

        private void OnCalibrationComplete()
        {
            if (!IsCalibrating)
            {
                return;
            }

            FinalizeCalibration(false, null);
        }

        private void OnOperatorMenuExit(OperatorMenuExitedEvent evt)
        {
            if (!IsCalibrating)
            {
                return;
            }

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
                        _activeCalibration?.Value.ViewModel.UpdateCalibration(e);
                        if (string.IsNullOrEmpty(e.ResourceKey))
                        {
                            return;
                        }

                        if (!string.IsNullOrEmpty(e.Error))
                        {
                            if (e.ResourceKey == ResourceKeys.TouchCalibrateModel)
                            {
                                var message = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TouchCalibrateWait));
                                _activeCalibration?.Value.ViewModel.UpdateError(message);
                            }
                            else
                            {
                                CalibrationError = true;
                                var error = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SerialCalibrationError), CalibrationErrorTimeoutSeconds);
                                _activeCalibration?.Value.ViewModel.UpdateError(error);
                                StartTimer(MilliSecondsPerSecond);
                            }
                        }
                        else
                        {
                            _activeCalibration?.Value.ViewModel.UpdateError(string.Empty);
                        }
                    });
            }
        }
    }
}
