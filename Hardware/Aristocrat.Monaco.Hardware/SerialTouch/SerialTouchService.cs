namespace Aristocrat.Monaco.Hardware.SerialTouch
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows;
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

    /// <summary>
    ///     SerialTouchService provides interaction with the serial touch device.
    /// </summary>
    /// <remarks>Supports the 3M MicroTouch serial protocol.  Currently assumes serial touch device is connected to COM16 (LS cabinet).</remarks>
    public class SerialTouchService : ISerialTouchService, IService, IDisposable
    {
        private const int CheckDisconnectTimeoutMilliSeconds = 5000;
        private const int MaxCheckDisconnectAttempts = 5;

        private const string CabinetTypeRegexLs = "^LS";

        private const string SerialTouchComPort = "COM3";
        private const int BaudRate = 9600;
        private const int DataBits = 8;
        private const int MaxBufferLength = 256;
        private const int CommunicationTimeoutMs = 1000;

        private const int CalibrationDelayMs = 2000; // Used to wait between calibration steps
        private const int MaxCoordinateRange = 16383; // 14 bits max

        private const int MaxTouchInfo = 1;
        private const int PointerId = 0;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly ISerialPortsService _serialPortsService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISerialPortController _serialPortController;

        private readonly PointerTouchInfo[] _pointerTouchInfo = new PointerTouchInfo[MaxTouchInfo];
        private readonly byte[] _response = new byte[MaxBufferLength];
        private readonly double _screenHeight;
        private readonly double _screenWidth;
        private readonly StateMachine<SerialTouchState, SerialTouchTrigger> _state;
        private readonly ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private bool _gotHeader;
        private int _dataIndex;
        private bool _touchDown;
        private bool _injectUpdate;
        private bool _resetForRecovery;
        private bool _checkDisconnect;
        private int _checkDisconnectAttempts;
        private bool _disposed;

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
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

            var success = InitializeTouchInjection(1, TouchFeedback.NONE);
            if (!success)
            {
                Logger.Error("Initialize - InitializeTouchInjection FAILED, returning");
                return;
            }

            if (!_serialPortController.IsEnabled)
            {
                var cabinetType = _cabinetDetectionService.Type;
                var match = Regex.Match(cabinetType.ToString(), CabinetTypeRegexLs, RegexOptions.None);
                if (match.Success)
                {
                    var port = _serialPortsService.LogicalToPhysicalName(SerialTouchComPort);
                    Logger.Debug($"Initialize -  Match LS cabinet, configuring: {port}");

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
                            KeepAliveTimeoutMs = CheckDisconnectTimeoutMilliSeconds
                        });

                    _serialPortController.IsEnabled = true; // This will open the COM port
                }
                else
                {
                    Logger.Warn("Initialize - No match LS cabinet, returning");
                    return;
                }
            }

            _eventBus.Subscribe<ExitRequestedEvent>(this, HandleExitRequested);

            Logger.Debug($"Initialize - Serial port open {_serialPortController.IsEnabled}");

            if (_serialPortController.IsEnabled)
            {
                _serialPortController.ReceivedData += OnDataReceived;
                _serialPortController.ReceivedError += OnErrorReceived;
                _serialPortController.KeepAliveExpired += OnCheckDisconnectTimeout;

                CrosshairColorLowerLeft = CalibrationCrosshairColors.Inactive;
                CrosshairColorUpperRight = CalibrationCrosshairColors.Inactive;

                // *NOTE* Serial touch will be initialized when the OutputIdentity is handled during the start-up sequence that will begin with this null command.
                SendNullCommand();
            }
            else if (!IsDisconnected)
            {
                Logger.Warn($"Initialize - Serial port {SerialTouchComPort} not available");
                Disconnected();
            }
        }

        /// <inheritdoc />
        public bool Initialized { get; private set; }

        /// <inheritdoc />
        public bool IsDisconnected { get; private set; }

        /// <inheritdoc />
        public string Model { get; private set; }

        /// <inheritdoc />
        public string OutputIdentity { get; private set; }

        /// <inheritdoc />
        public bool PendingCalibration { get; private set; }

        /// <inheritdoc />
        public void Reconnect(bool calibrating)
        {
            Logger.Debug($"Reconnect calibrating {calibrating}");
            _checkDisconnect = false;
            _checkDisconnectAttempts = 0;
            PendingCalibration = calibrating;
            CloseSerialPort();
            ClearResponse();
            _eventBus.UnsubscribeAll(this);
            Initialized = false;
            Initialize();
        }

        /// <inheritdoc />
        public void SendCalibrateExtendedCommand()
        {
            if (Fire(SerialTouchTrigger.CalibrateExtended))
            {
                _serialPortController.WriteBuffer(M3SerialTouchConstants.CalibrationCommand);
            }
        }

        /// <inheritdoc />
        public void SendDiagnosticCommand()
        {
            if (Fire(SerialTouchTrigger.Diagnostic))
            {
                _serialPortController.WriteBuffer(M3SerialTouchConstants.DiagnosticCommand);
            }
        }

        /// <inheritdoc />
        public void SendNameCommand()
        {
            if (Fire(SerialTouchTrigger.Name))
            {
                _serialPortController.WriteBuffer(M3SerialTouchConstants.NameCommand);
            }
        }

        /// <inheritdoc />
        public void SendNullCommand()
        {
            if (Fire(SerialTouchTrigger.Initialized))
            {
                _serialPortController.WriteBuffer(M3SerialTouchConstants.NullCommand);
            }
        }

        /// <inheritdoc />
        public void SendOutputIdentityCommand()
        {
            if (Fire(SerialTouchTrigger.OutputIdentity))
            {
                _serialPortController.WriteBuffer(M3SerialTouchConstants.OutputIdentityCommand);
            }
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
                    _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, string.Empty, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                }

                _serialPortController.WriteBuffer(M3SerialTouchConstants.ResetCommand);
            }
        }

        /// <inheritdoc />
        public void SendRestoreDefaultsCommand(bool calibrating = false)
        {
            if (Fire(SerialTouchTrigger.RestoreDefaults))
            {
                PendingCalibration = calibrating;
                _serialPortController.WriteBuffer(M3SerialTouchConstants.RestoreDefaultsCommand);
            }
        }

        /// <inheritdoc />
        public string Status { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                CloseSerialPort();
                _serialPortController.Dispose();
                _stateLock.Dispose();
            }

            _disposed = true;
        }

        private CalibrationCrosshairColors CrosshairColorLowerLeft { get; set; }

        private CalibrationCrosshairColors CrosshairColorUpperRight { get; set; }

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

        private void ClearResponse()
        {
            _gotHeader = false;
            Array.Clear(_response, 0, _response.Length);
            _dataIndex = 0;
        }

        private void CloseSerialPort()
        {
            _serialPortController.ReceivedData -= OnDataReceived;
            _serialPortController.ReceivedError -= OnErrorReceived;
            _serialPortController.KeepAliveExpired -= OnCheckDisconnectTimeout;
            _serialPortController.IsEnabled = false; // This will flush the in/out buffer, close and unregister the COM port
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
                .Permit(SerialTouchTrigger.Initialized, SerialTouchState.Null);
            stateMachine.Configure(SerialTouchState.Null)
                .PermitReentry(SerialTouchTrigger.Initialized)
                .Permit(SerialTouchTrigger.Name, SerialTouchState.Name)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.Name)
                .Permit(SerialTouchTrigger.OutputIdentity, SerialTouchState.OutputIdentity)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch);
            stateMachine.Configure(SerialTouchState.OutputIdentity)
                .Permit(SerialTouchTrigger.RestoreDefaults, SerialTouchState.RestoreDefaults)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch);
            stateMachine.Configure(SerialTouchState.RestoreDefaults)
                .Permit(SerialTouchTrigger.Reset, SerialTouchState.Reset)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.Reset)
                .Permit(SerialTouchTrigger.Uninitialized, SerialTouchState.Uninitialized)
                .Permit(SerialTouchTrigger.Diagnostic, SerialTouchState.Diagnostic)
                .Permit(SerialTouchTrigger.InterpretTouch, SerialTouchState.InterpretTouch)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.Diagnostic)
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
                .Permit(SerialTouchTrigger.Initialized, SerialTouchState.Null)
                .Permit(SerialTouchTrigger.RestoreDefaults, SerialTouchState.RestoreDefaults)
                .Permit(SerialTouchTrigger.Error, SerialTouchState.Error);
            stateMachine.Configure(SerialTouchState.Error)
                .Permit(SerialTouchTrigger.Initialized, SerialTouchState.Null)
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
                CrosshairColorLowerLeft = CalibrationCrosshairColors.Active;
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, ResourceKeys.TouchLowerLeftCrosshair, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                Status = "CALIBRATE EXTENDED...TOUCH LOWER LEFT CROSSHAIR";
                Fire(SerialTouchTrigger.LowerLeftTarget);
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                var error = status.ToString("X2");
                Status = $"ERROR - Calibrate Extended (CX) command failed with code {error}";
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(error, ResourceKeys.TouchCalibrateExtendedCommandFailed, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                _resetForRecovery = true;
                ClearResponse();
                SendResetCommand();
                Thread.Sleep(CalibrationDelayMs);
            }
        }

        private void HandleDiagnostic(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                Status = "DIAGNOSTIC COMMAND OK";
                if (_state.State > SerialTouchState.Diagnostic || PendingCalibration)
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
                Status = $"ERROR - Diagnostic (DX) command failed with code {status}";
                Reconnect(PendingCalibration);
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
            ClearResponse();
            SendResetCommand();
        }

        private void HandleLowerLeftTarget(byte status)
        {
            if (status == M3SerialTouchConstants.TargetAcknowledged)
            {
                CrosshairColorLowerLeft = CalibrationCrosshairColors.Acknowledged;
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, string.Empty, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                Status = "CALIBRATE EXTENDED LOWER LEFT OK";
                Thread.Sleep(CalibrationDelayMs);
                CrosshairColorLowerLeft = CalibrationCrosshairColors.Inactive;
                CrosshairColorUpperRight = CalibrationCrosshairColors.Active;
                Status = "CALIBRATE EXTENDED...TOUCH UPPER RIGHT CROSSHAIR";
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, ResourceKeys.TouchUpperRightCrosshair, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                Fire(SerialTouchTrigger.UpperRightTarget);
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                CrosshairColorLowerLeft = CalibrationCrosshairColors.Error;
                var error = status.ToString("X2");
                Status = $"ERROR - Calibrate Extended (CX) lower left target failed with code {error}";
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(error, ResourceKeys.TouchCalibrateExtendedLowerLeftTargetFailed, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                _resetForRecovery = true;
                ClearResponse();
                SendResetCommand();
                Thread.Sleep(CalibrationDelayMs);
            }
        }

        private void HandleName(byte[] response)
        {
            Model = GetModel(response);
            if (!Initialized)
            {
                SendOutputIdentityCommand();
            }
            else
            {
                Fire(SerialTouchTrigger.InterpretTouch);
            }
        }

        private void HandleNull(byte status)
        {
            if (status == M3SerialTouchConstants.StatusGood)
            {
                Status = "NULL COMMAND OK";
                if (!Initialized)
                {
                    SendNameCommand();
                }
                else
                {
                    Fire(SerialTouchTrigger.InterpretTouch);
                }
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                Status = $"ERROR - Null (Z) command failed with code {status}";
                Reconnect(PendingCalibration);
            }
        }

        private void HandleOutputIdentity(byte[] response)
        {
            OutputIdentity = Encoding.Default.GetString(response).TrimEnd();
            var controllerType = OutputIdentity.Substring(0, 2);
            var firmwareVersion = OutputIdentity.Substring(2, 4);
            Logger.Debug($"OutputIdentity - {controllerType} {firmwareVersion}");
            if (!Initialized)
            {
                Initialized = true;
                // *NOTE* We need to remap the touch screens so that the firmware information is updated for the mapped serial touch device.
                _cabinetDetectionService.MapTouchscreens();
                Logger.Info($"{Name} initialized");

                if (PendingCalibration)
                {
                    SendRestoreDefaultsCommand(true);
                    return;
                }
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
                    SendDiagnosticCommand();
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
                        _eventBus.Publish(new SerialTouchCalibrationStatusEvent(error, ResourceKeys.TouchCalibrateResetCommandFailed, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                        Thread.Sleep(CalibrationDelayMs);
                    }

                    _eventBus.Publish(new ExitRequestedEvent(ExitAction.Reboot));
                }
                else
                {
                    Fire(SerialTouchTrigger.Error);
                    Status = $"ERROR - Reset (R) command failed with code {status}";
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
                case SerialTouchState.RestoreDefaults:
                    HandleRestoreDefaults(response[0]);
                    break;
                case SerialTouchState.Reset:
                    HandleReset(response[0]);
                    break;
                case SerialTouchState.Diagnostic:
                    HandleDiagnostic(response[0]);
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
                    SendResetCommand(PendingCalibration);
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
                Reconnect(PendingCalibration);
            }
        }

        private void HandleUpperRightTarget(byte status)
        {
            if (status == M3SerialTouchConstants.TargetAcknowledged)
            {
                PendingCalibration = false;
                CrosshairColorUpperRight = CalibrationCrosshairColors.Acknowledged;
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, string.Empty, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                Status = "CALIBRATE EXTENDED UPPER RIGHT OK";
                Thread.Sleep(CalibrationDelayMs);
                CrosshairColorUpperRight = CalibrationCrosshairColors.Inactive;
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(string.Empty, ResourceKeys.CalibrationComplete, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                Thread.Sleep(CalibrationDelayMs);
                Status = "CALIBRATION COMPLETE";
                Fire(SerialTouchTrigger.InterpretTouch);
                _eventBus.Publish(new SerialTouchCalibrationCompletedEvent(true, string.Empty));
            }
            else
            {
                Fire(SerialTouchTrigger.Error);
                CrosshairColorUpperRight = CalibrationCrosshairColors.Error;
                var error = status.ToString("X2");
                Status = $"ERROR - Calibrate Extended (CX) upper right target failed with code {error}";
                _eventBus.Publish(new SerialTouchCalibrationStatusEvent(error, ResourceKeys.TouchCalibrateExtendedUpperRightTargetFailed, CrosshairColorLowerLeft, CrosshairColorUpperRight));
                _resetForRecovery = true;
                ClearResponse();
                SendResetCommand();
                Thread.Sleep(CalibrationDelayMs);
            }
        }

        /// <summary>
        ///     Inject touch input on injection target for given coordinate.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        private void InjectTouchCoordinate(int x, int y)
        {
            var state = _touchDown ? (_injectUpdate ? "UPDATE" : " DOWN ") : "  UP  ";
            if (_touchDown)
            {
                if (!_injectUpdate)
                {
                    _injectUpdate = true;
                    _pointerTouchInfo[PointerId] = CreateDefaultPointerTouchInfo(x, y, PointerId);
                    _pointerTouchInfo[PointerId].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                }
                else
                {
                    _pointerTouchInfo[PointerId].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                    UpdateContactArea(x, y, PointerId);
                }
            }
            else
            {
                _injectUpdate = false;
                _pointerTouchInfo[PointerId].PointerInfo.PointerFlags = PointerFlags.UP;
                UpdateContactArea(x, y, PointerId);
            }

            var injected = InjectTouchInput(MaxTouchInfo, _pointerTouchInfo);
            if (!injected)
            {
                Logger.Warn($"InjectTouchCoordinate - {state} - {x} {y} - FAILED INJECTION");
            }
        }

        private void InterpretTouch(IReadOnlyList<byte> response)
        {
            // Bit 7 of 1st status byte indicates proximity
            // 1 = Sensor is being touched (touchdown)
            // 0 = Sensor is not being touched (lift off)
            _touchDown = (response[0] & M3SerialTouchConstants.ProximityBit) != 0;

            // Horizontal coordinate data (X)
            var x = ConvertFormatTabletCoordinate(response[1], response[2], M3SerialTouchConstants.LowOrderBits);

            // Vertical coordinate data (Y)
            var y = ConvertFormatTabletCoordinate(response[3], response[4], M3SerialTouchConstants.LowOrderBits);

            var adjustX = MaxCoordinateRange / _screenWidth;
            var xCoord = Convert.ToInt32(Math.Round(x > 0 ? x / adjustX : 0));
            var adjustY = MaxCoordinateRange / _screenHeight;
            var flipY = MaxCoordinateRange - y;
            var yCoord = Convert.ToInt32(Math.Round(flipY > 0 ? flipY / adjustY : 0));

            InjectTouchCoordinate(xCoord, yCoord);
        }

        private void OnCheckDisconnectTimeout(object sender, EventArgs e)
        {
            if (_checkDisconnect && (_state.State == SerialTouchState.InterpretTouch || _state.State == SerialTouchState.Null))
            {
                if (_checkDisconnectAttempts > MaxCheckDisconnectAttempts)
                {
                    Logger.Warn($"OnCheckDisconnectTimeout - check disconnect exceeded max, attempting reconnect...");
                    Reconnect(PendingCalibration);
                    return;
                }

                if (_checkDisconnectAttempts > 0 && !IsDisconnected)
                {
                    Disconnected();
                }

                _checkDisconnectAttempts++;
                SendNullCommand();
            }
            else
            {
                _checkDisconnect = true;
            }
        }

        private void OnDataReceived(object sender, EventArgs e)
        {
            var bytes = new byte[_serialPortController.BytesToRead];
            if (_serialPortController.TryReadBuffer(ref bytes, 0, bytes.Length) < 1)
            {
                return;
            }

            if (IsDisconnected)
            {
                Connected();
            }

            _checkDisconnect = false;
            _checkDisconnectAttempts = 0;

            foreach (var b in bytes)
            {
                switch (b)
                {
                    case M3SerialTouchConstants.Header when !_gotHeader && _state.State != SerialTouchState.InterpretTouch:
                        _gotHeader = true;
                        break;
                    case M3SerialTouchConstants.Terminator when _state.State != SerialTouchState.InterpretTouch:
                        HandleResponse(_response);
                        ClearResponse();
                        break;
                    default:
                        ProcessTouchData(b);
                        break;
                }
            }
        }

        private void ProcessTouchData(byte b)
        {
            if (_dataIndex < MaxBufferLength)
            {
                _response[_dataIndex] = b;
            }

            if (_state.State == SerialTouchState.InterpretTouch &&
                _dataIndex == M3SerialTouchConstants.TouchDataLength)
            {
                HandleResponse(_response);
                ClearResponse();
            }
            else
            {
                _dataIndex++;
            }
        }

        private void OnErrorReceived(object sender, EventArgs e)
        {
            Logger.Error("OnErrorReceived");
            Reconnect(PendingCalibration);
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
    }
}