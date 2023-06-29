namespace Aristocrat.Monaco.Hardware.SerialTouch
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO.Ports;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Cabinet;
    using Common;
    using Contracts;
    using Contracts.Cabinet;
    using Contracts.Communicator;
    using Contracts.SerialPorts;
    using Contracts.SharedDevice;
    using Contracts.Touch;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using log4net;
    using NativeTouch;
    using Stateless;
    using static SerialTouchHelper;
    using Timer = System.Timers.Timer;
    using TouchAction = NativeTouch.TouchAction;

    /// <summary>
    ///     SerialTouchService provides interaction with the serial touch device.
    /// </summary>
    /// <remarks>
    ///     Supports the 3M MicroTouch serial protocol.  Currently assumes serial touch device is connected to COM16 (LS
    ///     cabinet).
    /// </remarks>
    public class SerialTouchService : ISerialTouchService, IService, IDisposable
    {
        private const int CheckDisconnectTimeoutMs = 10000;
        private const int MaxCheckDisconnectAttempts = 2;
        private const string SerialTouchComPort = "COM3";
        private const int BaudRate = 9600;
        private const int DataBits = 8;
        private const int MaxBufferLength = 256;
        private const int CommunicationTimeoutMs = 1000;
        private const int RequeueInterval = 200;
        private const int CalibrationDelayMs = 2000; // Used to wait between calibration steps
        private const int MaxTouchInfo = 1;
        private const int PointerId = 0;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly TimeSpan DisabledDelay = TimeSpan.FromMilliseconds(500);

        private readonly IEventBus _eventBus;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly ISerialPortsService _serialPortsService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISerialPortController _serialPortController;
        private readonly INativeTouch _nativeTouch;
        private readonly StateMachine<SerialTouchState, SerialTouchTrigger> _state;
        private readonly ReaderWriterLockSlim _stateLock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly object _lastUpdateLock = new();
        private readonly Timer _requeueTimer = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly TouchPacketBuilder _packetBuilder = new();

        private bool _resetForRecovery;
        private int _checkDisconnectAttempts;
        private bool _disposed;
        private bool _skipCalibrationPrompts; // Used to skip prompts for auto-calibrating device such as Kortek 130
        private byte[] _lastUpdatePacket = new byte[M3SerialTouchConstants.TouchDataLength];
        private TouchAction _previousTouchState;
        private bool _queueTasksCreated;
        private BlockingCollection<byte[]> _receiveQueue = new();
        private BlockingCollection<byte[]> _transmitQueue = new();

        public SerialTouchService(
            IEventBus eventBus,
            ICabinetDetectionService cabinetDetectionService,
            IPropertiesManager propertiesManager,
            ISerialPortsService serialPortsService,
            ISerialPortController serialPortController,
            INativeTouch nativeTouch)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _cabinetDetectionService = cabinetDetectionService ??
                                       throw new ArgumentNullException(nameof(cabinetDetectionService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _serialPortsService = serialPortsService ?? throw new ArgumentNullException(nameof(serialPortsService));
            _serialPortController = serialPortController ?? new SerialPortController(_serialPortsService);
            _nativeTouch = nativeTouch ?? throw new ArgumentNullException(nameof(nativeTouch));
            _state = CreateStateMachine();
        }

        public string FirmwareVersion { get; private set; }

        private CalibrationCrosshairColors CrosshairColorLowerLeft { get; set; }

        private CalibrationCrosshairColors CrosshairColorUpperRight { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool Initialized { get; private set; }

        /// <inheritdoc />
        public bool IsDisconnected { get; private set; }

        /// <inheritdoc />
        public string Model { get; private set; } = string.Empty;

        /// <inheritdoc />
        public string OutputIdentity { get; private set; }

        /// <inheritdoc />
        public bool PendingCalibration { get; private set; }

        public bool HasReceivedData { get; private set; }

        public bool InitializeTouchInjection()
        {
            return _nativeTouch.InitializeTouchInjection(MaxTouchInfo);
        }

        public void StartCalibration()
        {
            SendResetCommand(true);
        }

        public void CancelCalibration()
        {
            SendResetCommand();
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ISerialTouchService) };

        /// <inheritdoc />
        public void Initialize()
        {
            if (Initialized)
            {
                Logger.Warn("Initialize - Already initialized, returning");
                return;
            }

            // Have we disabled serial touch via command line?
            if (_propertiesManager.GetValue(HardwareConstants.SerialTouchDisabled, "false") == "true")
            {
                Logger.Warn("Initialize - Serial touch disabled via command line, returning");
                return;
            }

            if (!InitializeTouchInjection())
            {
                Logger.Error("Initialize - InitializeTouchInjection FAILED, returning");
                return;
            }

            if (!_cabinetDetectionService.ExpectedSerialTouchDevices.Any())
            {
                Logger.Warn("Initialize - No match serial touch devices expected, returning");
                return;
            }

            ConfigureComPort();
            _eventBus.Subscribe<ExitRequestedEvent>(this, HandleExitRequested);

            Logger.Debug($"Initialize - Serial port open {_serialPortController.IsEnabled}");
            if (_serialPortController.IsEnabled)
            {
                _serialPortController.ReceivedError += OnErrorReceived;
                _serialPortController.KeepAliveExpired += OnCheckDisconnectTimeout;

                CrosshairColorLowerLeft = CalibrationCrosshairColors.Inactive;
                CrosshairColorUpperRight = CalibrationCrosshairColors.Inactive;

                Fire(SerialTouchTrigger.Initialize);
            }
            else if (!IsDisconnected)
            {
                Logger.Warn($"Initialize - Serial port {SerialTouchComPort} not available");
                Disconnect();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _cts.Cancel();

                if (_requeueTimer != null)
                {
                    _requeueTimer.Stop();
                    _requeueTimer.Dispose();
                }

                _eventBus.UnsubscribeAll(this);
                CloseSerialPort();
                _serialPortController.Dispose();
                _stateLock.Dispose();

                if (_receiveQueue != null)
                {
                    _receiveQueue.Dispose();
                    _receiveQueue = null;
                }

                if (_transmitQueue != null)
                {
                    _transmitQueue.Dispose();
                    _transmitQueue = null;
                }

                _cts.Dispose();
            }

            _disposed = true;
        }

        private static string GetModel(byte[] response)
        {
            return Encoding.Default.GetString(response).TrimEnd('\0');
        }

        private static IReadOnlyDictionary<string, object> GetDeviceDetails(string deviceName)
        {
            var deviceDetails = new Dictionary<string, object>
            {
                { nameof(BaseDeviceEvent.DeviceId), deviceName ?? string.Empty },
                { nameof(BaseDeviceEvent.DeviceCategory), "SERIAL" }
            };

            return deviceDetails;
        }

        private void SendResetCommand(bool calibrating = false)
        {
            if (!Fire(SerialTouchTrigger.Reset))
            {
                return;
            }

            PendingCalibration = calibrating;
            if (PendingCalibration)
            {
                CrosshairColorLowerLeft = CalibrationCrosshairColors.Inactive;
                CrosshairColorUpperRight = CalibrationCrosshairColors.Inactive;
                _eventBus.Publish(
                    new SerialTouchCalibrationStatusEvent(
                        string.Empty,
                        string.Empty,
                        CrosshairColorLowerLeft,
                        CrosshairColorUpperRight));
            }

            _transmitQueue?.Add(M3SerialTouchConstants.ResetCommand);
        }

        private bool CanFire(SerialTouchTrigger trigger)
        {
            _stateLock.EnterReadLock();
            try
            {
                var canFire = _state.CanFire(trigger);
                if (!canFire)
                {
                    Logger.Debug($"CanFire - FAILED for trigger {trigger} in state {_state.State}");
                }

                return canFire;
            }
            finally
            {
                _stateLock.ExitReadLock();
            }
        }

        private void ConfigureComPort()
        {
            Logger.Debug("SerialTouchService - Is LS cabinet");

            var port = _serialPortsService.LogicalToPhysicalName(SerialTouchComPort);
            var keepAlive = CheckDisconnectTimeoutMs;
            Logger.Debug($"Initialize -  Match LS cabinet, configuring {port} with KeepAliveTimeoutMs {keepAlive}");

            _serialPortController.UseSyncMode = true;
            _serialPortController.Configure(
                new ComConfiguration
                {
                    PortName = port,
                    Mode = ComConfiguration.RS232CommunicationMode,
                    BaudRate = BaudRate,
                    DataBits = DataBits,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadBufferSize = MaxBufferLength,
                    WriteBufferSize = MaxBufferLength,
                    ReadTimeoutMs = SerialPort.InfiniteTimeout,
                    WriteTimeoutMs = CommunicationTimeoutMs,
                    KeepAliveTimeoutMs = keepAlive
                });

            _serialPortController.IsEnabled = true;

            if (!_queueTasksCreated)
            {
                _queueTasksCreated = true;

                _requeueTimer.Interval = RequeueInterval;
                _requeueTimer.AutoReset = true;
                _requeueTimer.Elapsed += OnRequeueTimer;

                Task.Run(() => ProcessReceiveQueue(_cts.Token), _cts.Token)
                    .FireAndForget(ex => Logger.Error($"ProcessReceiveQueue: Exception occurred {ex}"));

                SendAndReceiveData(_cts.Token)
                    .FireAndForget(ex => Logger.Error($"SendAndReceiveData: Exception occurred {ex}"));
            }
        }

        private void CloseSerialPort()
        {
            _serialPortController.ReceivedError -= OnErrorReceived;
            _serialPortController.KeepAliveExpired -= OnCheckDisconnectTimeout;
            _serialPortController.IsEnabled =
                false; // This will flush the in/out buffer, close and unregister the COM port
            _packetBuilder.Reset();
        }

        private StateMachine<SerialTouchState, SerialTouchTrigger> CreateStateMachine()
        {
            var stateMachine = new StateMachine<SerialTouchState, SerialTouchTrigger>(SerialTouchState.Uninitialized);
            stateMachine.Configure(SerialTouchState.Uninitialized)
                .Permit(SerialTouchTrigger.Name, SerialTouchState.Name)
                .Permit(SerialTouchTrigger.Initialize, SerialTouchState.Initialize);
            stateMachine.Configure(SerialTouchState.Initialize)
                .OnEntry(() => _transmitQueue?.Add(M3SerialTouchConstants.ResetCommand))
                .Permit(SerialTouchTrigger.Name, SerialTouchState.Name);
            stateMachine.Configure(SerialTouchState.Name)
                .OnEntry(SendNameCommand)
                .PermitReentry(SerialTouchTrigger.Name)
                .Permit(SerialTouchTrigger.OutputIdentity, SerialTouchState.OutputIdentity)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch);
            stateMachine.Configure(SerialTouchState.Null)
                .OnEntry(SendNullCommand)
                .PermitReentry(SerialTouchTrigger.Null)
                .Permit(SerialTouchTrigger.NullCompleted, SerialTouchState.InterpretTouch)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.OutputIdentity)
                .OnEntry(SendOutputIdentityCommand)
                .Permit(SerialTouchTrigger.Reset, SerialTouchState.Reset)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch);
            stateMachine.Configure(SerialTouchState.Reset)
                .Permit(SerialTouchTrigger.Uninitialized, SerialTouchState.Uninitialized)
                .Permit(SerialTouchTrigger.RestoreDefaults, SerialTouchState.RestoreDefaults)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.RestoreDefaults)
                .Permit(SerialTouchTrigger.CalibrateExtended, SerialTouchState.CalibrateExtended)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.CalibrateExtended)
                .Permit(SerialTouchTrigger.Reset, SerialTouchState.Reset)
                .Permit(SerialTouchTrigger.LowerLeftTarget, SerialTouchState.LowerLeftTarget)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.LowerLeftTarget)
                .Permit(SerialTouchTrigger.Reset, SerialTouchState.Reset)
                .Permit(SerialTouchTrigger.UpperRightTarget, SerialTouchState.UpperRightTarget)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.UpperRightTarget)
                .Permit(SerialTouchTrigger.Reset, SerialTouchState.Reset)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.InterpretTouch)
                .Permit(SerialTouchTrigger.Null, SerialTouchState.Null)
                .Permit(SerialTouchTrigger.Reset, SerialTouchState.Reset)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.Error)
                .Permit(SerialTouchTrigger.Name, SerialTouchState.Name)
                .Permit(SerialTouchTrigger.Null, SerialTouchState.Null)
                .Permit(SerialTouchTrigger.Reset, SerialTouchState.Reset)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch);

            stateMachine.OnUnhandledTrigger(
                (state, trigger) => { Logger.Error($"Invalid State {state} For Trigger {trigger}"); });

            stateMachine.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"Trigger {transition.Trigger} Transitioned From {transition.Source} To {transition.Destination}");
                });

            return stateMachine;
        }

        private void Disconnect()
        {
            IsDisconnected = true;
            CloseSerialPort();
            _eventBus.Publish(new DeviceDisconnectedEvent(GetDeviceDetails(Model)));
            HasReceivedData = false;
        }

        private bool Fire(SerialTouchTrigger trigger)
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (!CanFire(trigger))
                {
                    Logger.Debug($"Fire - FAILED CanFire for trigger {trigger}");
                    return false;
                }

                _state.Fire(trigger);
                return true;
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void HandleCalibrateExtended(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                if (!_skipCalibrationPrompts)
                {
                    CrosshairColorLowerLeft = CalibrationCrosshairColors.Active;
                    _eventBus.Publish(
                        new SerialTouchCalibrationStatusEvent(
                            string.Empty,
                            ResourceKeys.TouchLowerLeftCrosshair,
                            CrosshairColorLowerLeft,
                            CrosshairColorUpperRight));
                }

                Fire(SerialTouchTrigger.LowerLeftTarget);
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                var error = status.ToString("X2", CultureInfo.InvariantCulture);
                Logger.Error(
                    $"HandleCalibrateExtended - ERROR - Calibrate Extended (CX) command failed with code {error}");
                _eventBus.Publish(
                    new SerialTouchCalibrationStatusEvent(
                        error,
                        ResourceKeys.TouchCalibrateExtendedCommandFailed,
                        CrosshairColorLowerLeft,
                        CrosshairColorUpperRight));
                _resetForRecovery = true;
                SendResetCommand(PendingCalibration);
                Thread.Sleep(CalibrationDelayMs);
            }
        }

        private void HandleExitRequested(ExitRequestedEvent e)
        {
            // Did we handle an exit request to restart and we are pending calibration?
            if (e.ExitAction != ExitAction.Restart || !PendingCalibration)
            {
                return;
            }

            // Yes, send a reset to attempt to kick serial touch device out of calibration before we exit.
            // *NOTE* If this reset fails, we will request to reboot instead.
            Fire(SerialTouchTrigger.Error);
            _resetForRecovery = true;
            SendResetCommand();
        }

        private void HandleLowerLeftTarget(byte status)
        {
            if (status == M3SerialTouchConstants.TargetAcknowledged)
            {
                if (!_skipCalibrationPrompts)
                {
                    CrosshairColorLowerLeft = CalibrationCrosshairColors.Acknowledged;
                    _eventBus.Publish(
                        new SerialTouchCalibrationStatusEvent(
                            string.Empty,
                            string.Empty,
                            CrosshairColorLowerLeft,
                            CrosshairColorUpperRight));
                    Thread.Sleep(CalibrationDelayMs);
                    CrosshairColorLowerLeft = CalibrationCrosshairColors.Inactive;
                    CrosshairColorUpperRight = CalibrationCrosshairColors.Active;
                    _eventBus.Publish(
                        new SerialTouchCalibrationStatusEvent(
                            string.Empty,
                            ResourceKeys.TouchUpperRightCrosshair,
                            CrosshairColorLowerLeft,
                            CrosshairColorUpperRight));
                }

                Fire(SerialTouchTrigger.UpperRightTarget);
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                var error = status.ToString("X2", CultureInfo.InvariantCulture);
                Logger.Error(
                    $"HandleLowerLeftTarget - ERROR - Calibrate Extended (CX) lower left target failed with code {error}");
                CrosshairColorLowerLeft = CalibrationCrosshairColors.Error;
                _eventBus.Publish(
                    new SerialTouchCalibrationStatusEvent(
                        error,
                        ResourceKeys.TouchCalibrateExtendedLowerLeftTargetFailed,
                        CrosshairColorLowerLeft,
                        CrosshairColorUpperRight));
                _resetForRecovery = true;
                SendResetCommand(PendingCalibration);
                Thread.Sleep(CalibrationDelayMs);
            }
        }

        private void HandleNull(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                Fire(SerialTouchTrigger.NullCompleted);
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                Logger.Error($"HandleNull - ERROR - Null (Z) command failed with code {status}");
            }
        }

        private void HandleName(byte[] response)
        {
            Model = GetModel(response);
            if (string.IsNullOrEmpty(Model) && !Initialized)
            {
                Logger.Warn("HandleName - Name null or empty, re-sending...");
                Fire(SerialTouchTrigger.Name);
                return;
            }

            Logger.Debug($"HandleName - {Model}");
            if (Model.Contains("Kortek"))
            {
                _skipCalibrationPrompts = true;
            }

            Fire(!Initialized ? SerialTouchTrigger.OutputIdentity : SerialTouchTrigger.InterpretTouch);
        }

        private void HandleOutputIdentity(byte[] response)
        {
            const int identityLength = 6;
            const int productLength = 2;

            OutputIdentity = Encoding.Default.GetString(response).TrimEnd();
            var controllerType = OutputIdentity.Substring(0, 2);
            FirmwareVersion = OutputIdentity.Substring(2, 4);
            Logger.Debug($"HandleOutputIdentity - {controllerType} {FirmwareVersion}");
            if (!Initialized)
            {
                Initialized = true;
                var version = 0;
                var product = "?";

                if (OutputIdentity.Length >= identityLength)
                {
                    product = OutputIdentity.Substring(0, productLength);
                    version = int.TryParse(OutputIdentity.Substring(productLength), out var result) ? result : 0;
                }

                var touchDevice = _cabinetDetectionService.ExpectedTouchDevices.FirstOrDefault(
                    x => x.CommunicationType == CommunicationTypes.Serial);

                _cabinetDetectionService.UpdateTouchDevice(
                    touchDevice,
                    new TouchDeviceUpdates(Model, product, version));
                Logger.Info(
                    $"HandleOutputIdentity - {Name} {FirmwareVersion} initialized.  {OutputIdentity}, {version}");
            }

            Fire(SerialTouchTrigger.InterpretTouch);
        }

        private void HandleInitialize(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                Fire(SerialTouchTrigger.Name);
            }
            else
            {
                if (Fire(SerialTouchTrigger.Uninitialized))
                {
                    Fire(SerialTouchTrigger.Initialize);
                }
            }
        }

        private void HandleReset(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                if (_resetForRecovery)
                {
                    _resetForRecovery = false;
                    if (Fire(SerialTouchTrigger.Uninitialized))
                    {
                        Fire(SerialTouchTrigger.Initialize);
                    }
                }
                else if (PendingCalibration)
                {
                    SendRestoreDefaultsCommand();
                }
                else
                {
                    Fire(SerialTouchTrigger.InterpretTouch);
                }
            }
            else
            {
                if (_resetForRecovery)
                {
                    Fire(SerialTouchTrigger.Error);
                    _resetForRecovery = false;
                    if (PendingCalibration)
                    {
                        var error = status.ToString("X2", CultureInfo.InvariantCulture);
                        Logger.Error($"HandleReset - ERROR - Reset (R) command failed with code {error}");
                        _eventBus.Publish(
                            new SerialTouchCalibrationStatusEvent(
                                error,
                                ResourceKeys.TouchCalibrateResetCommandFailed,
                                CrosshairColorLowerLeft,
                                CrosshairColorUpperRight));
                        Thread.Sleep(CalibrationDelayMs);
                    }

                    _eventBus.Publish(new ExitRequestedEvent(ExitAction.Reboot));
                }
                else
                {
                    Fire(SerialTouchTrigger.Error);
                    Logger.Error($"HandleReset - ERROR - Reset (R) command failed with code {status}");
                }
            }
        }

        private void HandleResponse(byte[] response)
        {
            switch (_state.State)
            {
                case SerialTouchState.Null:
                    HandleNull(response[0]);
                    break;
                case SerialTouchState.Name:
                    HandleName(response);
                    break;
                case SerialTouchState.OutputIdentity:
                    HandleOutputIdentity(response);
                    break;
                case SerialTouchState.Reset:
                    HandleReset(response[0]);
                    break;
                case SerialTouchState.Initialize:
                    HandleInitialize(response[0]);
                    break;
                case SerialTouchState.RestoreDefaults:
                    HandleRestoreDefaults(response[0]);
                    break;
                case SerialTouchState.CalibrateExtended:
                    HandleCalibrateExtended(response[0]);
                    break;
                case SerialTouchState.LowerLeftTarget:
                    HandleLowerLeftTarget(response[0]);
                    break;
                case SerialTouchState.UpperRightTarget:
                    HandleUpperRightTarget(response[0]);
                    break;
                case SerialTouchState.InterpretTouch:
                    if (IsSyncBitSet(response[0]))
                    {
                        HandleTouch(response);
                    }
                    else
                    {
                        Logger.Warn($"HandleResponse - [{response.ToHexString()}] Sync bit not set, response ignored");
                    }

                    break;
            }
        }

        private void HandleRestoreDefaults(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                if (PendingCalibration)
                {
                    SendCalibrateExtendedCommand();
                }
                else
                {
                    Fire(SerialTouchTrigger.InterpretTouch);
                }
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                Logger.Error(
                    $"HandleRestoreDefaults - ERROR - Restore Defaults (RD) command failed with code {status}");
            }
        }

        private void HandleUpperRightTarget(byte status)
        {
            if (status == M3SerialTouchConstants.TargetAcknowledged)
            {
                PendingCalibration = false; // Done calibrating
                if (!_skipCalibrationPrompts)
                {
                    CrosshairColorUpperRight = CalibrationCrosshairColors.Acknowledged;
                    _eventBus.Publish(
                        new SerialTouchCalibrationStatusEvent(
                            string.Empty,
                            string.Empty,
                            CrosshairColorLowerLeft,
                            CrosshairColorUpperRight));
                    Thread.Sleep(CalibrationDelayMs);
                    CrosshairColorUpperRight = CalibrationCrosshairColors.Inactive;
                }

                _eventBus.Publish(
                    new SerialTouchCalibrationStatusEvent(
                        string.Empty,
                        ResourceKeys.CalibrationComplete,
                        CrosshairColorLowerLeft,
                        CrosshairColorUpperRight));
                Thread.Sleep(CalibrationDelayMs);
                Fire(SerialTouchTrigger.InterpretTouch);
                _eventBus.Publish(new SerialTouchCalibrationCompletedEvent(true, string.Empty));
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                var error = status.ToString("X2", CultureInfo.InvariantCulture);
                Logger.Error(
                    $"HandleUpperRightTarget - ERROR - Calibrate Extended (CX) upper right target failed with code {error}");
                CrosshairColorUpperRight = CalibrationCrosshairColors.Error;
                _eventBus.Publish(
                    new SerialTouchCalibrationStatusEvent(
                        error,
                        ResourceKeys.TouchCalibrateExtendedUpperRightTargetFailed,
                        CrosshairColorLowerLeft,
                        CrosshairColorUpperRight));
                _resetForRecovery = true;
                SendResetCommand(PendingCalibration);
                Thread.Sleep(CalibrationDelayMs);
            }
        }

        private void HandleTouch(byte[] touchPacket)
        {
            var touchState = IsUpTouchDataPacket(touchPacket) ? TouchAction.Up : TouchAction.Down;
            var action = TouchAction.Update;
            if (touchState != _previousTouchState)
            {
                action = touchState;
            }

            _previousTouchState = touchState;
            _requeueTimer.Stop();
            var errors = _nativeTouch.InjectTouchInputs(new[] { GetPointerTouchInfo(touchPacket, action, PointerId) });
            if (errors != InjectionErrors.None)
            {
                if (errors == InjectionErrors.InvalidParameters)
                {
                    _previousTouchState = TouchAction.Up;
                    lock (_lastUpdateLock)
                    {
                        Array.Clear(_lastUpdatePacket, 0, M3SerialTouchConstants.TouchDataLength);
                    }
                }

                Logger.Debug(
                    $"InjectTouchCoordinate - Last injection failed: {errors}, Packet: [{touchPacket.ToHexString()}]");
            }

            _requeueTimer.Start();
        }

        private void OnCheckDisconnectTimeout(object sender, EventArgs e)
        {
            if (_checkDisconnectAttempts > MaxCheckDisconnectAttempts && !IsDisconnected)
            {
                Logger.Warn("OnCheckDisconnectTimeout - check disconnect exceeded max, disconnecting");
                Disconnect();
                return;
            }

            if (!Initialized)
            {
                Logger.Warn("OnCheckDisconnectTimeout - Not initialized, incrementing disconnect counter");
                _checkDisconnectAttempts++;
                return;
            }

            if (PendingCalibration)
            {
                Logger.Warn("OnCheckDisconnectTimeout - pending calibration, returning...");
                return;
            }

            if (_state.State != SerialTouchState.InterpretTouch && _state.State != SerialTouchState.Null)
            {
                Logger.Warn($"OnCheckDisconnectTimeout - Skipped while in state {_state.State}, returning...");
                return;
            }

            _checkDisconnectAttempts++;
            Fire(SerialTouchTrigger.Null);
        }

        private void OnRequeueTimer(object sender, ElapsedEventArgs e)
        {
            // If windows touch does not receive an update regularly it will timeout and cancel touch.
            // This will keep it alive by sending the last update again if we don't get data from the serial port.
            lock (_lastUpdateLock)
            {
                if (!IsValidTouchDataPacket(_lastUpdatePacket))
                {
                    _requeueTimer.Stop();
                    return;
                }

                Logger.Warn("Adding last update to prevent timeout.");
                _receiveQueue?.Add(_lastUpdatePacket);
            }
        }

        private void QueueLiftoffFromLastUpdate()
        {
            if (IsValidTouchDataPacket(_lastUpdatePacket))
            {
                Logger.Debug("QueueLiftoffFromLastUpdate - Adding last update packet as liftoff to response queue");
                _receiveQueue?.Add(ToggleProximityBit(_lastUpdatePacket));
                Array.Clear(_lastUpdatePacket, 0, M3SerialTouchConstants.TouchDataLength);
            }
            else
            {
                Logger.Error("QueueLiftoffFromLastUpdate - Unexpected data - Last update is empty");
            }
        }

        private async Task SendAndReceiveData(CancellationToken token)
        {
            var portReadTask = HandlePortReads(token);
            var queueTask = GetReceiveData(token);
            while (!token.IsCancellationRequested)
            {
                if (!_serialPortController.IsEnabled)
                {
                    await Task.Delay(DisabledDelay, token).ConfigureAwait(false);
                    continue;
                }

                // Read current data
                var result = await Task.WhenAny(portReadTask, queueTask).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (result == portReadTask)
                {
                    var data = await result.ConfigureAwait(false);
                    if (data.Any())
                    {
                        ProcessNewData(data);
                        _checkDisconnectAttempts = 0;
                    }

                    portReadTask = HandlePortReads(token);
                }
                else
                {
                    var message = await result.ConfigureAwait(false);
                    await ProcessMessage(message, token).ConfigureAwait(false);
                    queueTask = GetReceiveData(token);
                }
            }
        }

        private async Task ProcessMessage(byte[] message, CancellationToken token)
        {
            Logger.Debug($"SendAndReceiveData - Removing [{message.ToHexString()}] from transmit queue");
            _packetBuilder.Reset();
            _serialPortController.FlushInputAndOutput();
            await _serialPortController.WriteAsync(message, 0, message.Length, token).ConfigureAwait(false);
        }

        private async Task<byte[]> GetReceiveData(CancellationToken token)
        {
            try
            {
                return await Task.Run(() => _transmitQueue?.Take(token) ?? Array.Empty<byte>(), token)
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }
        }

        private async Task<byte[]> HandlePortReads(CancellationToken token)
        {
            try
            {
                var result = new byte[1];
                var count = await _serialPortController.ReadAsync(result, 0, 1, token).ConfigureAwait(false);
                return count > 0 ? result : Array.Empty<byte>();
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }
        }

        private void ProcessReceiveQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var packet = _receiveQueue?.Take(token);
                if (packet == null)
                {
                    return;
                }

                if (_state.State == SerialTouchState.InterpretTouch && !IsValidTouchDataPacket(packet) ||
                    _state.State != SerialTouchState.InterpretTouch && !IsValidCommandResponsePacket(packet))
                {
                    Logger.Error(
                        $"ProcessReceiveQueue - Unexpected packet [{packet.ToHexString()}] in state {_state.State}");
                    continue;
                }

                if (IsValidCommandResponsePacket(packet))
                {
                    packet = StripHeaderAndTerminator(packet);
                }

                HandleResponse(packet);

                HasReceivedData = true;
            }
        }

        private void ProcessNewData(byte[] bytes)
        {
            try
            {
                _packetBuilder.Append(bytes);
            }
            catch (AggregateException e)
            {
                foreach (var innerException in e.InnerExceptions.OfType<InvalidPacketException>())
                {
                    var lostPacket = innerException.PacketUnderConstruction;
                    Logger.Error(
                        $"ProcessNewData - Unexpected data received - New byte: [{innerException.NextByte.ToHexString()}], Existing packet: [{lostPacket.ToHexString()}], State: {_state.State}");

                    if (IsUpTouchDataPacket(lostPacket))
                    {
                        QueueLiftoffFromLastUpdate();
                    }
                }
            }

            ProcessAvailablePackets();
        }

        private void ProcessAvailablePackets()
        {
            if (!_packetBuilder.TryTakePackets(out var packets))
            {
                return;
            }

            foreach (var packet in packets)
            {
                if (IsUpTouchDataPacket(packet))
                {
                    QueueLiftoffFromLastUpdate();
                    continue;
                }

                Logger.Debug($"ProcessNewData - Adding [{packet.ToHexString()}] to response queue");
                _receiveQueue?.Add(packet);

                if (IsDownTouchDataPacket(packet))
                {
                    lock (_lastUpdateLock)
                    {
                        _lastUpdatePacket = packet;
                    }
                }
            }
        }

        private void OnErrorReceived(object sender, EventArgs e)
        {
            Logger.Error("OnErrorReceived");
            Disconnect();
        }

        private void SendCalibrateExtendedCommand()
        {
            if (Fire(SerialTouchTrigger.CalibrateExtended))
            {
                _transmitQueue?.Add(M3SerialTouchConstants.CalibrateExtendedCommand);
            }
        }

        private void SendNameCommand()
        {
            _transmitQueue?.Add(M3SerialTouchConstants.NameCommand);
        }

        private void SendNullCommand()
        {
            _transmitQueue?.Add(M3SerialTouchConstants.NullCommand);
        }

        private void SendOutputIdentityCommand()
        {
            _transmitQueue?.Add(M3SerialTouchConstants.OutputIdentityCommand);
        }

        private void SendRestoreDefaultsCommand()
        {
            if (Fire(SerialTouchTrigger.RestoreDefaults))
            {
                _transmitQueue?.Add(M3SerialTouchConstants.RestoreDefaultsCommand);
            }
        }
    }
}