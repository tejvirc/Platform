namespace Aristocrat.Monaco.Hardware.SerialTouch
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Windows;
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
    using Stateless;
    using static NativeMethods;
    using Timer = System.Timers.Timer;

    /// <summary>
    ///     SerialTouchService provides interaction with the serial touch device.
    /// </summary>
    /// <remarks>Supports the 3M MicroTouch serial protocol.  Currently assumes serial touch device is connected to COM16 (LS cabinet).</remarks>
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
        private const int MaxCoordinateRange = 16383; // 14 bits max
        private const int MaxTouchInfo = 1;
        private const int PointerId = 0;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly ISerialPortsService _serialPortsService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISerialPortController _serialPortController;
        private readonly PointerTouchInfo[] _pointerTouchInfo = new PointerTouchInfo[MaxTouchInfo];
        private readonly double _screenHeight;
        private readonly double _screenWidth;
        private readonly StateMachine<SerialTouchState, SerialTouchTrigger> _state;
        private readonly ReaderWriterLockSlim _stateLock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly bool _isLsCabinet;
        private readonly object _touchDataLock = new();
        private readonly Timer _requeueTimer = new();
        private readonly CancellationTokenSource _cts = new();

        private bool _touchDown;
        private bool _resetForRecovery;
        private int _checkDisconnectAttempts;
        private bool _disposed;
        private bool _skipCalibrationPrompts; // Used to skip prompts for auto-calibrating device such as Kortek 130
        private List<byte> _partialPacket = new();
        private List<byte> _lastUpdatePacket = new();
        private int _previousTouchState;
        private bool _queueTasksCreated;
        private BlockingCollection<byte[]> _receiveQueue = new();
        private BlockingCollection<byte[]> _transmitQueue = new();

        public SerialTouchService()
            : this(ServiceManager.GetInstance().TryGetService<IEventBus>(),
                   ServiceManager.GetInstance().TryGetService<ICabinetDetectionService>(),
                   ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
                   ServiceManager.GetInstance().TryGetService<ISerialPortsService>(),
                   new SerialPortController())
        {
        }

        public SerialTouchService(
            IEventBus eventBus,
            ICabinetDetectionService cabinetDetectionService,
            IPropertiesManager propertiesManager,
            ISerialPortsService serialPortsService,
            ISerialPortController serialPortController)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _cabinetDetectionService = cabinetDetectionService ?? throw new ArgumentNullException(nameof(cabinetDetectionService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _serialPortsService = serialPortsService ?? throw new ArgumentNullException(nameof(serialPortsService));
            _serialPortController = serialPortController ?? throw new ArgumentNullException(nameof(serialPortController));

            _screenWidth = SystemParameters.PrimaryScreenWidth;
            _screenHeight = SystemParameters.PrimaryScreenHeight;
            Logger.Debug($"SerialTouchService - _screenWidth {_screenWidth} _screenHeight {_screenHeight}");

            _state = CreateStateMachine();
            _isLsCabinet = _cabinetDetectionService.IsCabinetType(HardwareConstants.CabinetTypeRegexLs);

            if (_isLsCabinet)
            {
                ConfigureComPort();
            }
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ISerialTouchService) };

        /// <inheritdoc />
        public bool IsManualTabletInputService { get; private set; }

        /// <inheritdoc />
        public bool Initialized { get; private set; }

        /// <inheritdoc />
        public bool IsDisconnected { get; private set; }

        public string FirmwareVersion { get; private set; }

        /// <inheritdoc />
        public string Model { get; private set; }

        /// <inheritdoc />
        public string OutputIdentity { get; private set; }

        /// <inheritdoc />
        public bool PendingCalibration { get; private set; }

        /// <inheritdoc />
        public string Status { get; set; }

        private CalibrationCrosshairColors CrosshairColorLowerLeft { get; set; }

        private CalibrationCrosshairColors CrosshairColorUpperRight { get; set; }

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

            if (!_isLsCabinet)
            {
                Logger.Warn("Initialize - No match LS cabinet, returning");
                return;
            }

            _eventBus.Subscribe<ExitRequestedEvent>(this, HandleExitRequested);
            
            Logger.Debug($"Initialize - Serial port open {_serialPortController.IsEnabled}");
            if (_serialPortController.IsEnabled)
            {
                _serialPortController.ReceivedError += OnErrorReceived;
                _serialPortController.KeepAliveExpired += OnCheckDisconnectTimeout;

                CrosshairColorLowerLeft = CalibrationCrosshairColors.Inactive;
                CrosshairColorUpperRight = CalibrationCrosshairColors.Inactive;

                // *NOTE* Serial touch will be initialized when the OutputIdentity is handled during the start-up sequence that will begin with this Name command.
                SendNameCommand();
            }
            else if (!IsDisconnected)
            {
                Logger.Warn($"Initialize - Serial port {SerialTouchComPort} not available");
                Disconnected();
            }
        }

        public void ClearPartialPacket()
        {
            lock (_touchDataLock)
            {
                _partialPacket.Clear();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void SendResetCommand(bool calibrating = false)
        {
            if (Fire(SerialTouchTrigger.Reset))
            {
                PendingCalibration = calibrating;
                if (PendingCalibration)
                {
                    CrosshairColorLowerLeft = CalibrationCrosshairColors.Inactive;
                    CrosshairColorUpperRight = CalibrationCrosshairColors.Inactive;
                    _eventBus.Publish(new SerialTouchCalibrationStatusEvent(Model + " " + FirmwareVersion, ResourceKeys.TouchCalibrateModel, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                }
                
                _transmitQueue?.Add(M3SerialTouchConstants.ResetCommand);
            }
        }

        public bool InitializeTouchInjection()
        {
            return NativeMethods.InitializeTouchInjection(1, TouchFeedback.NONE);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _cts.Cancel(true);

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

        private void Reconnect(bool calibrating)
        {
            Logger.Debug($"Reconnect - calibrating {calibrating}");
            _checkDisconnectAttempts = 0;
            PendingCalibration = calibrating;
            CloseSerialPort();
            _eventBus.UnsubscribeAll(this);
            Initialized = false;
            Initialize();
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
            CheckTabletInputServiceStartup();

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

                Task.Run(() => { ProcessReceiveQueue(_cts.Token); }, _cts.Token)
                    .FireAndForget(ex => Logger.Error($"ProcessReceiveQueue: Exception occurred {ex}"));

                Task.Run(() => { SendAndReceiveData(_cts.Token); }, _cts.Token)
                    .FireAndForget(ex => Logger.Error($"SendAndReceiveData: Exception occurred {ex}"));
            }
        }

        private void CheckTabletInputServiceStartup()
        {
            ConnectionOptions connectionOptions = new ConnectionOptions { Impersonation = ImpersonationLevel.Impersonate };
            ManagementScope scope = new ManagementScope(@"\\" + Environment.MachineName + @"\root\cimv2")
            {
                Options = connectionOptions
            };
            SelectQuery query = new SelectQuery("select * from Win32_Service");

            using ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection collection = searcher.Get();
            foreach (var o in collection)
            {
                var service = (ManagementObject)o;
                if (service.Properties["Name"].Value.ToString() == "TabletInputService" &&
                    service.Properties["StartMode"].Value.Equals("Manual"))
                {
                    Logger.Debug($"CheckTabletInputServiceStartup - Found {service.Properties["Name"].Value} with StartMode {service.Properties["StartMode"].Value}");
                    IsManualTabletInputService = true;
                }
            }
        }

        private void CloseSerialPort()
        {
            _serialPortController.ReceivedError -= OnErrorReceived;
            _serialPortController.KeepAliveExpired -= OnCheckDisconnectTimeout;
            _serialPortController.IsEnabled = false; // This will flush the in/out buffer, close and unregister the COM port
            ClearPartialPacket();
        }

        private void Connected()
        {
            IsDisconnected = false;
            _eventBus.Publish(new DeviceConnectedEvent(GetDeviceDetails(SerialTouchComPort)));
        }

        /// <summary>
        ///     Converts the given format tablet bytes to corresponding coordinate.
        /// </summary>
        /// <param name="b1">The first byte.</param>
        /// <param name="b2">The second byte.</param>
        /// <param name="maskLowOrderBits">The bitmask for the low order bits of first byte.</param>
        /// <returns>A value in the range of 0 to 16383.</returns>
        /// <remarks>The corresponding coordinate value is represented by the first 7 bits of each byte, so 14 bits total.</remarks>
        private static short ConvertFormatTabletCoordinate(byte b1, byte b2, byte maskLowOrderBits)
        {
            // Mask off the synchronization bit and low-order bits (insignificant) of 1st byte
            var maskedByte1 = (byte)(b1 & ~(M3SerialTouchConstants.SyncBit | maskLowOrderBits));

            // Mask off the synchronization bit of 2nd byte
            var maskedByte2 = (byte)(b2 & ~M3SerialTouchConstants.SyncBit);

            // Is the 1st bit of masked byte 2 set?
            if ((maskedByte2 & (1 << 0)) != 0)
            {
                // Yes, set the synchronization bit of maskedByte1.
                maskedByte1 |= M3SerialTouchConstants.SyncBit;
            }

            // Shift 2nd byte to the right 1 bit.
            maskedByte2 >>= 1;

            // Combine the two masked bytes for a value in the range of 0 to 16383 (14 bits).
            var value = (short)((maskedByte2 << 8) | maskedByte1);

            return value;
        }

        private static PointerTouchInfo CreateDefaultPointerTouchInfo(int x, int y, uint id)
        {
            var pointerTouchInfo = new PointerTouchInfo();
            pointerTouchInfo.PointerInfo.pointerType = PointerInputType.TOUCH;
            pointerTouchInfo.PointerInfo.PointerId = id;
            pointerTouchInfo.TouchFlags = TouchFlags.NONE;
            pointerTouchInfo.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
            pointerTouchInfo.PointerInfo.PtPixelLocation.X = x;
            pointerTouchInfo.PointerInfo.PtPixelLocation.Y = y;
            pointerTouchInfo.ContactArea.left = x - M3SerialTouchConstants.TouchRadius;
            pointerTouchInfo.ContactArea.right = x + M3SerialTouchConstants.TouchRadius;
            pointerTouchInfo.ContactArea.top = y - M3SerialTouchConstants.TouchRadius;
            pointerTouchInfo.ContactArea.bottom = y + M3SerialTouchConstants.TouchRadius;
            pointerTouchInfo.Orientation = M3SerialTouchConstants.TouchOrientation;
            pointerTouchInfo.Pressure = M3SerialTouchConstants.TouchPressure;
            return pointerTouchInfo;
        }

        private StateMachine<SerialTouchState, SerialTouchTrigger> CreateStateMachine()
        {
            var stateMachine = new StateMachine<SerialTouchState, SerialTouchTrigger>(SerialTouchState.Uninitialized);
            stateMachine.Configure(SerialTouchState.Uninitialized)
                .Permit(SerialTouchTrigger.Name, SerialTouchState.Name);
            stateMachine.Configure(SerialTouchState.Name)
                .PermitReentry(SerialTouchTrigger.Name)
                .Permit(SerialTouchTrigger.OutputIdentity, SerialTouchState.OutputIdentity)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch);
            stateMachine.Configure(SerialTouchState.Null)
                .PermitReentry(SerialTouchTrigger.Null)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.OutputIdentity)
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
                .Permit(SerialTouchTrigger.LowerLeftTarget, SerialTouchState.LowerLeftTarget)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.LowerLeftTarget)
                .Permit(SerialTouchTrigger.UpperRightTarget, SerialTouchState.UpperRightTarget)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.UpperRightTarget)
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
                (state, trigger) =>
                {
                    Logger.Error($"Invalid State {state} For Trigger {trigger}");
                });

            stateMachine.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"Trigger {transition.Trigger} Transitioned From {transition.Source} To {transition.Destination}");
                });

            return stateMachine;
        }

        private void Disconnected()
        {
            IsDisconnected = true;
            _eventBus.Publish(new DeviceDisconnectedEvent(GetDeviceDetails(SerialTouchComPort)));
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

        private static IDictionary<string, object> GetDeviceDetails(string deviceName)
        {
            var deviceDetails = new Dictionary<string, object>
            {
                { nameof(BaseDeviceEvent.DeviceId), deviceName },
                { nameof(BaseDeviceEvent.DeviceCategory), "SERIAL" }
            };

            return deviceDetails;
        }

        private void HandleCalibrateExtended(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                if (!_skipCalibrationPrompts)
                {
                    CrosshairColorLowerLeft = CalibrationCrosshairColors.Active;
                    _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, ResourceKeys.TouchLowerLeftCrosshair, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                }

                Status = "CALIBRATE EXTENDED...TOUCH LOWER LEFT CROSSHAIR";
                Fire(SerialTouchTrigger.LowerLeftTarget);
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                var error = status.ToString("X2");
                Status = $"ERROR - Calibrate Extended (CX) command failed with code {error}";
                Logger.Error($"HandleCalibrateExtended - {Status}");
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(error, ResourceKeys.TouchCalibrateExtendedCommandFailed, CrosshairColorLowerLeft, CrosshairColorUpperRight));
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
                    _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, string.Empty, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                    Status = "CALIBRATE EXTENDED LOWER LEFT OK";
                    Thread.Sleep(CalibrationDelayMs);
                    CrosshairColorLowerLeft = CalibrationCrosshairColors.Inactive;
                    CrosshairColorUpperRight = CalibrationCrosshairColors.Active;
                    Status = "CALIBRATE EXTENDED...TOUCH UPPER RIGHT CROSSHAIR";
                    _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, ResourceKeys.TouchUpperRightCrosshair, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                }

                Fire(SerialTouchTrigger.UpperRightTarget);
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                var error = status.ToString("X2");
                Status = $"ERROR - Calibrate Extended (CX) lower left target failed with code {error}";
                Logger.Error($"HandleLowerLeftTarget - {Status}");
                CrosshairColorLowerLeft = CalibrationCrosshairColors.Error;
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(error, ResourceKeys.TouchCalibrateExtendedLowerLeftTargetFailed, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                _resetForRecovery = true;
                SendResetCommand(PendingCalibration);
                Thread.Sleep(CalibrationDelayMs);
            }
        }

        private void HandleNull(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                Status = "NULL COMMAND OK";
                Fire(SerialTouchTrigger.InterpretTouch);
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                Status = $"ERROR - Null (Z) command failed with code {status}";
                Logger.Error($"HandleNull - {Status}");
                Reconnect(PendingCalibration);
            }
        }

        private void HandleName(byte[] response)
        {
            Model = GetModel(response);
            if (string.IsNullOrEmpty(Model) && !Initialized)
            {
                Logger.Warn("HandleName - Name null or empty, re-sending...");
                SendNameCommand();
                return;
            }

            Logger.Debug($"HandleName - {Model}");
            if (Model.Contains("Kortek"))
            {
                _skipCalibrationPrompts = true;
            }

            if (!Initialized)
            {
                SendOutputIdentityCommand();
            }
            else
            {
                Fire(SerialTouchTrigger.InterpretTouch);
            }
        }

        private void HandleOutputIdentity(byte[] response)
        {
            OutputIdentity = Encoding.Default.GetString(response).TrimEnd();
            var controllerType = OutputIdentity.Substring(0, 2);
            FirmwareVersion = OutputIdentity.Substring(2, 4);
            Logger.Debug($"HandleOutputIdentity - {controllerType} {FirmwareVersion}");
            if (!Initialized)
            {
                Initialized = true;
                // *NOTE* We need to remap the touch screens so that the firmware information is updated for the mapped serial touch device.
                _cabinetDetectionService.MapTouchscreens();
                Logger.Info($"HandleOutputIdentity - {Name} {FirmwareVersion} initialized");
                SendResetCommand();
                return;
            }

            Fire(SerialTouchTrigger.InterpretTouch);
        }

        private void HandleReset(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                Status = "RESET COMMAND OK";
                if (_resetForRecovery)
                {
                    _resetForRecovery = false;
                    Fire(SerialTouchTrigger.Uninitialized);
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
                        var error = status.ToString("X2");
                        Status = $"ERROR - Reset (R) command failed with code {error}";
                        Logger.Error($"HandleReset - {Status}");
                        _eventBus.Publish(new SerialTouchCalibrationStatusEvent(error, ResourceKeys.TouchCalibrateResetCommandFailed, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                        Thread.Sleep(CalibrationDelayMs);
                    }

                    _eventBus.Publish(new ExitRequestedEvent(ExitAction.Reboot));
                }
                else
                {
                    Fire(SerialTouchTrigger.Error);
                    Status = $"ERROR - Reset (R) command failed with code {status}";
                    Logger.Error($"HandleReset - {Status}");
                    Reconnect(PendingCalibration);
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
                    // Sync bit of 1st status byte should always be 1
                    if ((response[0] & M3SerialTouchConstants.SyncBit) != 0)
                    {
                        InterpretTouch(response);
                    }
                    else
                    {
                        var b1 = response[0].ToString("X2");
                        Logger.Warn($"HandleResponse - {b1} {Convert.ToString(response[0], 2).PadLeft(8, '0')} - Sync bit not set, response ignored");
                    }

                    break;
            }
        }

        private void HandleRestoreDefaults(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                Status = "RESTORE DEFAULTS COMMAND OK";
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
                Status = $"ERROR - Restore Defaults (RD) command failed with code {status}";
                Logger.Error($"HandleRestoreDefaults - {Status}");
                Reconnect(PendingCalibration);
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
                    _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, string.Empty, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                    Status = "CALIBRATE EXTENDED UPPER RIGHT OK";
                    Thread.Sleep(CalibrationDelayMs);
                    CrosshairColorUpperRight = CalibrationCrosshairColors.Inactive;
                }

                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, ResourceKeys.CalibrationComplete, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                Thread.Sleep(CalibrationDelayMs);
                Status = "CALIBRATION COMPLETE";
                Fire(SerialTouchTrigger.InterpretTouch);
                _eventBus.Publish(new SerialTouchCalibrationCompletedEvent(true, string.Empty));
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                var error = status.ToString("X2");
                Status = $"ERROR - Calibrate Extended (CX) upper right target failed with code {error}";
                Logger.Error($"HandleUpperRightTarget - {Status}");
                CrosshairColorUpperRight = CalibrationCrosshairColors.Error;
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(error, ResourceKeys.TouchCalibrateExtendedUpperRightTargetFailed, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                _resetForRecovery = true;
                SendResetCommand(PendingCalibration);
                Thread.Sleep(CalibrationDelayMs);
            }
        }

        private void InjectTouchCoordinate(int x, int y, TouchAction action, IReadOnlyList<byte> response)
        {
            bool injected;

            if (action == TouchAction.Down)
            {
                _pointerTouchInfo[PointerId] = CreateDefaultPointerTouchInfo(x, y, PointerId);
                _pointerTouchInfo[PointerId].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                injected = InjectTouchInput(MaxTouchInfo, _pointerTouchInfo);
            }
            else if (action == TouchAction.Up)
            {
                _pointerTouchInfo[PointerId].PointerInfo.PointerFlags = PointerFlags.UP;
                injected = InjectTouchInput(MaxTouchInfo, _pointerTouchInfo);
            }
            else
            {
                _pointerTouchInfo[PointerId].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                UpdateContactArea(x, y, PointerId);
                injected = InjectTouchInput(MaxTouchInfo, _pointerTouchInfo);
            }

            if (!injected)
            {
                var errorCode = Marshal.GetLastWin32Error();
                string errorText = ((SystemErrors)errorCode).ToString();

                if ((SystemErrors)errorCode == SystemErrors.ERROR_INVALID_PARAMETER)
                {
                    _previousTouchState = 0;

                    lock (_touchDataLock)
                    {
                        _lastUpdatePacket.Clear();
                    }
                }

                Logger.Debug($"InjectTouchCoordinate - Last injection failed: {errorText}, Packet: { BytesToHex(response.ToArray()) }");
            }
            else
            {
                lock (_touchDataLock)
                {
                    if (action is TouchAction.Down or TouchAction.Update)
                    {
                        _lastUpdatePacket = response.ToList();
                    }
                    else
                    {
                        _lastUpdatePacket.Clear();
                    }
                }
            }
        }

        private void InterpretTouch(IReadOnlyList<byte> response)
        {
            // Bit 7 of 1st status byte indicates proximity
            // 1 = Sensor is being touched (touchdown)
            // 0 = Sensor is not being touched (lift off)
            var touchState = (response[0] & M3SerialTouchConstants.ProximityBit) == 0 ? 0 : 1;

            TouchAction action = TouchAction.Update;
            if (touchState != _previousTouchState)
            {
                action = (TouchAction)touchState;
            }

            _previousTouchState = touchState;
            _touchDown = (response[0] & M3SerialTouchConstants.ProximityBit) != 0;

            // Horizontal coordinate data (X)parity
            var x = ConvertFormatTabletCoordinate(response[1], response[2], M3SerialTouchConstants.LowOrderBits);

            // Vertical coordinate data (Y)
            var y = ConvertFormatTabletCoordinate(response[3], response[4], M3SerialTouchConstants.LowOrderBits);

            var adjustX = MaxCoordinateRange / _screenWidth;
            var xCoord = Convert.ToInt32(Math.Round(x > 0 ? x / adjustX : 0));
            var adjustY = MaxCoordinateRange / _screenHeight;
            var flipY = MaxCoordinateRange - y;
            var yCoord = Convert.ToInt32(Math.Round(flipY > 0 ? flipY / adjustY : 0));

            _requeueTimer.Stop();
            InjectTouchCoordinate(xCoord, yCoord, action, response);
            _requeueTimer.Start();
        }

        private void OnCheckDisconnectTimeout(object sender, EventArgs e)
        {
            if (!Initialized)
            {
                Logger.Warn("OnCheckDisconnectTimeout - Not initialized, returning...");
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

            if (_touchDown)
            {
                Logger.Warn("OnCheckDisconnectTimeout - Skipped while touch down, returning...");
                return;
            }

            if (_checkDisconnectAttempts > MaxCheckDisconnectAttempts)
            {
                Logger.Warn("OnCheckDisconnectTimeout - check disconnect exceeded max, attempting reconnect...");
                if (!IsDisconnected)
                {
                    Logger.Warn("OnCheckDisconnectTimeout - Disconnected, attempting re-connect...");
                    Disconnected();
                }

                Reconnect(false);
                return;
            }

            _checkDisconnectAttempts++;
            SendNullCommand();
        }

        private void BuildResponsePackets(byte[] bytes)
        {
            foreach (var currentByte in bytes)
            {
                lock (_touchDataLock)
                {
                    // Command responses
                    if (_state.State != SerialTouchState.InterpretTouch)
                    {
                        // Unexpected data - Throw it out
                        if (!_partialPacket.Any() && currentByte != M3SerialTouchConstants.Header || _partialPacket.Any() && _partialPacket[0] != M3SerialTouchConstants.Header)
                        {
                            DropBadData(currentByte);

                            if (currentByte != M3SerialTouchConstants.Header)
                            {
                                continue;
                            }
                        }

                        // Add expected data
                        _partialPacket.Add(currentByte);
                        if (currentByte == M3SerialTouchConstants.Terminator)
                        {
                            QueueFullPacket();
                        }

                        continue;
                    }
                    
                    // Touch response
                    // Unexpected data - Throw it out
                    if (_partialPacket.Any() &&
                        (currentByte & M3SerialTouchConstants.SyncBit) == M3SerialTouchConstants.SyncBit)
                    {
                        if ((_partialPacket[0] & M3SerialTouchConstants.ProximityBit) == 0)
                        {
                            DropBadData(currentByte);
                            QueueLiftoffFromLastUpdate();
                        }
                        else
                        {
                            DropBadData(currentByte);
                        }
                    }
                    
                    // Add expected data
                    _partialPacket.Add(currentByte);
                    if (_partialPacket.Count == M3SerialTouchConstants.TouchDataLength)
                    {
                        if ((_partialPacket[0] & M3SerialTouchConstants.ProximityBit) == 0)
                        {
                            QueueLiftoffFromLastUpdate();
                        }
                        else
                        {
                            QueueFullPacket();
                        }
                    }
                }
            }

            void DropBadData(byte currentByte)
            {
                Logger.Error($"Unexpected data received - State: {_state.State}, New byte: {BytesToHex(new []{ currentByte })}, Existing packet: {BytesToHex(_partialPacket.ToArray())}");
                _partialPacket.Clear();
            }
        }

        private void OnRequeueTimer(object sender, ElapsedEventArgs e)
        {
            // If windows touch does not receive an update regularly it will timeout and cancel touch.
            // This will keep it alive by sending the last update again if we don't get data from the serial port.
            lock (_touchDataLock)
            {
                if (!_lastUpdatePacket.Any())
                {
                    _requeueTimer.Stop();
                    return;
                }

                if (_partialPacket.Any() && (_partialPacket[0] & M3SerialTouchConstants.ProximityBit) == 0)
                {
                    Logger.Warn("Adding liftoff early to prevent timeout.");
                    QueueLiftoffFromLastUpdate();
                }
                else
                {
                    Logger.Warn("Adding last update to prevent timeout.");
                    _receiveQueue?.Add(_lastUpdatePacket.ToArray());
                }
            }
        }

        private void QueueLiftoffFromLastUpdate()
        {
            // Use the data from the last update as the liftoff point.
            if (_lastUpdatePacket.Any())
            {
                _partialPacket = new List<byte>(_lastUpdatePacket);
                _partialPacket[0] = (byte)(_partialPacket[0] & ~M3SerialTouchConstants.ProximityBit);
                QueueFullPacket();
            }
            else
            {
                Logger.Error("QueueLiftoffFromLastUpdate - Unexpected data - Last update is empty");
            }
        }

        private void QueueFullPacket()
        {
            Logger.Debug($"QueueFullPacket - Adding {BytesToHex(_partialPacket.ToArray())} to response queue");
            _receiveQueue?.Add(_partialPacket.ToArray());
            _partialPacket.Clear();
        }

        private void SendAndReceiveData(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_serialPortController.IsEnabled)
                {
                    // Read current data
                    var bytes = new byte[_serialPortController.BytesToRead];
                    if (_serialPortController.TryReadBuffer(ref bytes, 0, bytes.Length) > 0)
                    {
                        BuildResponsePackets(bytes);

                        if (IsDisconnected)
                        {
                            Logger.Debug("SendAndReceiveData - Connected");
                            Connected();
                        }

                        _checkDisconnectAttempts = 0;
                    }

                    // Transmit pending messages
                    if (_transmitQueue != null && _transmitQueue.TryTake(out var message))
                    {
                        Logger.Debug($"SendAndReceiveData - Removing {BytesToHex(message)} from transmit queue");

                        ClearPartialPacket();
                        _serialPortController.FlushInputAndOutput();
                        _serialPortController.WriteBuffer(message);
                    }
                }
            }
        }

        private void ProcessReceiveQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var responsePacket = _receiveQueue?.Take(token);
                if (responsePacket == null)
                {
                    return;
                }

                // Strip the header and terminator for command responses
                if (_state.State != SerialTouchState.InterpretTouch && 
                    responsePacket[0] == M3SerialTouchConstants.Header &&
                    responsePacket[responsePacket.Length - 1] == M3SerialTouchConstants.Terminator)
                {
                    responsePacket = responsePacket.Skip(1).Take(responsePacket.Length - 2).ToArray();
                }

                HandleResponse(responsePacket);
            }
        }

        private void OnErrorReceived(object sender, EventArgs e)
        {
            Logger.Error("OnErrorReceived");
            Reconnect(PendingCalibration);
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
            if (Fire(SerialTouchTrigger.Name))
            {
                _transmitQueue?.Add(M3SerialTouchConstants.NameCommand);
            }
        }

        private void SendNullCommand()
        {
            if (Fire(SerialTouchTrigger.Null))
            {
                _transmitQueue?.Add(M3SerialTouchConstants.NullCommand);
            }
        }

        private void SendOutputIdentityCommand()
        {
            if (Fire(SerialTouchTrigger.OutputIdentity))
            {
                _transmitQueue?.Add(M3SerialTouchConstants.OutputIdentityCommand);
            }
        }

        private void SendRestoreDefaultsCommand()
        {
            if (Fire(SerialTouchTrigger.RestoreDefaults))
            {
                _transmitQueue?.Add(M3SerialTouchConstants.RestoreDefaultsCommand);
            }
        }

        private void UpdateContactArea(int x, int y, uint id)
        {
            _pointerTouchInfo[id].PointerInfo.PtPixelLocation.X = x;
            _pointerTouchInfo[id].PointerInfo.PtPixelLocation.Y = y;
            _pointerTouchInfo[id].ContactArea.left = x - M3SerialTouchConstants.TouchRadius;
            _pointerTouchInfo[id].ContactArea.right = x + M3SerialTouchConstants.TouchRadius;
            _pointerTouchInfo[id].ContactArea.top = y - M3SerialTouchConstants.TouchRadius;
            _pointerTouchInfo[id].ContactArea.bottom = y + M3SerialTouchConstants.TouchRadius;
        }

        private static string GetModel(byte[] response)
        {
            return Encoding.Default.GetString(response).TrimEnd('\0');
        }

        private static string BytesToHex(byte[] bytes)
        {
            return string.Join(":", bytes.Select(x => x.ToString("X2")));
        }

        private enum TouchAction
        {
            Up = 0,
            Down = 1,
            Update = 2
        }
    }
}