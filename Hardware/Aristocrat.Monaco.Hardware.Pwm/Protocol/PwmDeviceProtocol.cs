namespace Aristocrat.Monaco.Hardware.Pwm.Protocol
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Timers;
    using Contracts.Communicator;
    using Contracts.Gds;
    using Contracts.PWM;
    using Contracts.SharedDevice;
    using log4net;
    using Microsoft.Win32.SafeHandles;
    using Timer = System.Timers.Timer;

    /// <summary>Class to manage protocol for pulse width devices </summary>
    public abstract class PwmDeviceProtocol : IGdsCommunicator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected static readonly Timer PollTimer = new ();
        protected static readonly AutoResetEvent Poll = new AutoResetEvent(true);

        private readonly char[] _device = new char[256];
        private NativeOverlapped _overlapped;
        private SafeFileHandle _deviceHandle;
        private IntPtr _dataPtr;
        private bool _waitingForRx;
        private bool _disposed;
        private Thread _monitoringThread;

        protected PwmDeviceConfig DeviceConfig { get; set; }
        protected bool Running;

        /// <summary>Create instance of PwmDeviceProtocol</summary>
        protected PwmDeviceProtocol()
        {
            _overlapped = new NativeOverlapped()
            {
                OffsetLow = 0,
                OffsetHigh = 0,
                EventHandle = NativeMethods.CreateEvent(IntPtr.Zero, Convert.ToInt32(false), Convert.ToInt32(false), "")
            };
            _dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ChangeRecord)));
            _waitingForRx = false;
            _disposed = false;
        }

        /// <inheritdoc/>
        public virtual string Manufacturer { get; set; }

        /// <inheritdoc/>
        public virtual string Model { get; set; }

        /// <inheritdoc/>
        public string Firmware => string.Empty;

        /// <inheritdoc/>
        public string SerialNumber => string.Empty;

        /// <inheritdoc/>
        public bool IsOpen { get; set; }

        /// <inheritdoc/>
        public int VendorId { get; set; } = -1;

        /// <inheritdoc/>
        public int ProductId { get; set; } = -1;

        /// <inheritdoc/>
        public int ProductIdDfu { get; set; } = -1;

        /// <inheritdoc/>
        public string Protocol => "Pwm";

        /// <inheritdoc/>
        public DeviceType DeviceType { get; set; }

        /// <inheritdoc/>
        public IDevice Device { get; set; }

        /// <inheritdoc/>
        public string FirmwareVersion => string.Empty;

        /// <inheritdoc/>
        public string FirmwareRevision => string.Empty;

        /// <inheritdoc/>
        public int FirmwareCrc => -1;

        /// <inheritdoc/>
        public string BootVersion => string.Empty;

        /// <inheritdoc/>
        public string VariantName => string.Empty;

        /// <inheritdoc/>
        public string VariantVersion => string.Empty;

        /// <inheritdoc/>
        public bool IsDfuCapable => false;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> DeviceAttached;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> DeviceDetached;

        /// <inheritdoc/>
        public event EventHandler<GdsSerializableMessage> MessageReceived;

        /// <inheritdoc/>
        public bool Close()
        {
            _ = StopDeviceMonitoring();
            if (_deviceHandle != null && !_deviceHandle.IsInvalid)
            {
                NativeMethods.CancelIoEx(_deviceHandle, IntPtr.Zero);
                _deviceHandle.Close();
                _deviceHandle.SetHandleAsInvalid();
                IsOpen = false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool Open()
        {
            if (GetDevice())
            {
                if (OpenDevice())
                {
                    IsOpen = StartDeviceMonitoring();
                }
            }
            return IsOpen;
        }

        /// <inheritdoc/>
        public void ResetConnection()
        {
            // Method intentionally left empty.
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public abstract bool Configure(IComConfiguration comConfiguration);

        /// <inheritdoc/>
        public abstract void SendMessage(GdsSerializableMessage message, CancellationToken token);

        /// <summary>Raises the <see cref="DeviceAttached"/> event.</summary>
        protected virtual void OnDeviceAttached()
        {
            DeviceAttached?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="DeviceDetached"/> event.</summary>
        protected virtual void OnDeviceDetached()
        {
            //Reset cached FirmwareCrc to UnknownCrc, so that the latest crc will be read on device attach again.
            DeviceDetached?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="MessageReceived"/> event.</summary>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if sent.</returns>
        protected virtual bool OnMessageReceived(GdsSerializableMessage message)
        {
            var invoker = MessageReceived;
            if (null != invoker)
            {
                invoker.Invoke(this, message);
                return true;
            }
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }
            Close();
            _disposed = true;
        }

        /// <summary>Sync read</summary>
        /// <returns>Tuple of data and read status. <see cref="ChangeRecord"</returns>
        protected virtual (bool, ChangeRecord) ReadSync()
        {
            if (_deviceHandle.IsInvalid)
            {
                return (false, new ChangeRecord());
            }

            unsafe
            {
                var size = Marshal.SizeOf(typeof(ChangeRecord));
                var success = NativeMethods.ReadFile(
                  _deviceHandle,
                  _dataPtr,
                  (uint)size,
                  out _,
                  IntPtr.Zero);

                if (success)
                {
                    var record = (ChangeRecord)Marshal.PtrToStructure(_dataPtr, typeof(ChangeRecord));
                    Marshal.FreeHGlobal(_dataPtr);
                    _dataPtr = Marshal.AllocHGlobal(size);
                    return (true, record);
                }
            }
            return (false, new ChangeRecord());
        }

        /// <summary>Async read</summary>
        /// <returns>Tuple of data and read status. <see cref="ChangeRecord"</returns>
        protected virtual (bool, ChangeRecord) ReadAsync()
        {
            if (_deviceHandle.IsInvalid)
            {
                return (false, new ChangeRecord());
            }

            if (DeviceConfig.Mode != CreateFileOption.Overlapped)
            {
                throw new InvalidOperationException();
            }

            unsafe
            {
                uint numberOfBytesRead = 0;
                var size = Marshal.SizeOf(typeof(ChangeRecord));
                if (!_waitingForRx)
                {
                    var status = NativeMethods.ReadFile(_deviceHandle,
                             _dataPtr,
                             (uint)size,
                             out numberOfBytesRead,
                             ref _overlapped);
                    if (status)
                    {
                        _waitingForRx = false;
                    }
                    else
                    {
                        _waitingForRx = true;
                    }
                }

                if (_waitingForRx)
                {
                    var result = NativeMethods.WaitForSingleObject(_overlapped.EventHandle, DeviceConfig.waitPeriod);

                    switch (result)
                    {
                        case NativeConstants.WaitObject0:
                            _waitingForRx = false;
                            NativeMethods.GetOverlappedResult(_deviceHandle, ref _overlapped, out numberOfBytesRead, false);
                            break;
                        case NativeConstants.WaitTimeout:
                        default:
                            _waitingForRx = true;
                            break;
                    }
                }

                if (!_waitingForRx && numberOfBytesRead > 0)
                {
                    var record = (ChangeRecord)Marshal.PtrToStructure(_dataPtr, typeof(ChangeRecord));
                    Marshal.FreeHGlobal(_dataPtr);
                    _dataPtr = Marshal.AllocHGlobal(size);
                    return (true, record);
                }
            }
            return (false, new ChangeRecord());
        }

        /// <summary>Read call to pwm device</summary>
        /// <returns>Tuple of data and read status. <see cref="ChangeRecord"</returns>
        protected virtual (bool, ChangeRecord) Read()
        {

            return DeviceConfig.Mode == CreateFileOption.Overlapped ? ReadAsync() : ReadSync();
        }

        /// <summary>IOCTL call to pwm device</summary>
        /// <param name="command">command to device</param>
        /// <param name="data">input.</param>
        /// <returns>True if success. <see cref="ChangeRecord"</returns>
        protected bool Ioctl<T, C>(C command, T data) where C : struct, IConvertible
        {
            if (_deviceHandle.IsInvalid)
            {
                return false;
            }

            if (!typeof(C).IsEnum)
            {
                throw new InvalidOperationException();
            }

            var dataHandle = default(GCHandle);
            var inputPtr = IntPtr.Zero;
            try
            {
                if (data != null)
                {
                    dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    inputPtr = dataHandle.AddrOfPinnedObject();
                }
                var result = NativeMethods.DeviceIoControl(_deviceHandle,
                    NativeConstants.PWMDEVICE_MAKE_IOCTL(DeviceConfig.DeviceType, command),
                            inputPtr,
                            (uint)Marshal.SizeOf(data),
                            IntPtr.Zero,
                            0,
                            out _,
                            IntPtr.Zero);
                return result;
            }
            finally
            {
                if (inputPtr != IntPtr.Zero)
                {
                    dataHandle.Free();
                }
            }
        }

        /// <summary>Aysnc IOCTL call to pwm device</summary>
        /// <param name="command">command to device.</param>
        /// <param name="data">input.</param>
        /// <param name="outData">output.</param>
        /// <returns>True if success. <see cref="ChangeRecord"</returns>
        protected bool IoctlAsync<T, C, R>(C command, T data, ref R outData) where C : struct, IConvertible
        {
            if (_deviceHandle.IsInvalid)
            {
                return false;
            }
            if (DeviceConfig.Mode != CreateFileOption.Overlapped)
            {
                throw new InvalidOperationException();
            }

            unsafe
            {
                uint numberOfBytesRead = 0;
                var size = Marshal.SizeOf(typeof(ChangeRecord));
                if (!_waitingForRx)
                {
                    var status = NativeMethods.DeviceIoControl(_deviceHandle,
                NativeConstants.PWMDEVICE_MAKE_IOCTL(DeviceConfig.DeviceType, command),
                IntPtr.Zero,
                Marshal.SizeOf(data),
                _dataPtr,
                (uint)size,
                out numberOfBytesRead,
                ref _overlapped);
                    if (status)
                    {
                        _waitingForRx = false;
                    }
                    else
                    {
                        _waitingForRx = true;
                    }
                }


                if (_waitingForRx)
                {
                    var result = NativeMethods.WaitForSingleObject(_overlapped.EventHandle, DeviceConfig.waitPeriod);

                    switch (result)
                    {
                        case NativeConstants.WaitObject0:
                            _waitingForRx = false;
                            NativeMethods.GetOverlappedResult(_deviceHandle, ref _overlapped, out numberOfBytesRead, false);
                            break;
                        case NativeConstants.WaitTimeout:
                        default:
                            _waitingForRx = true;
                            break;
                    }
                }

                if (!_waitingForRx && numberOfBytesRead > 0)
                {
                    outData = (R)Marshal.PtrToStructure(_dataPtr, outData.GetType());
                    Marshal.FreeHGlobal(_dataPtr);
                    _dataPtr = Marshal.AllocHGlobal(size);
                    return true;
                }
            }
            return false;
        }

        /// <summary>IOCTL call to pwm device</summary>
        /// <param name="command">command to device.</param>
        /// <param name="data">input.</param>
        /// <param name="outData">output.</param>
        /// <returns>True if success. <see cref="ChangeRecord"</returns>
        protected bool Ioctl<T, C, R>(C command, T data, ref R outData) where C : struct, IConvertible
        {
            if (_deviceHandle.IsInvalid)
            {
                return false;
            }

            if (!typeof(C).IsEnum)
            {
                throw new InvalidOperationException();
            }

            var dataHandle = default(GCHandle);
            var inputPtr = IntPtr.Zero;
            var outputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(data));

            try
            {
                if (data != null)
                {
                    dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    inputPtr = dataHandle.AddrOfPinnedObject();
                }

                var result = NativeMethods.DeviceIoControl(_deviceHandle,
                    NativeConstants.PWMDEVICE_MAKE_IOCTL(DeviceConfig.DeviceType, command),
                            inputPtr,
                            (uint)Marshal.SizeOf(data),
                            outputPtr,
                            (uint)Marshal.SizeOf(outData),
                            out _,
                            IntPtr.Zero);

                outData = (R)Marshal.PtrToStructure(outputPtr, outData.GetType());
                return result;
            }
            finally
            {
                if (inputPtr != IntPtr.Zero)
                {
                    dataHandle.Free();
                }

                if (outputPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(outputPtr);
                }
            }
        }

        /// <summary>call to start monitoring thread for pwm device</summary>
        /// <returns>True if success. <see cref="ChangeRecord"</returns>
        protected bool StartDeviceMonitoring()
        {
            Running = true;
            _monitoringThread = new Thread(Run)
            {
                Name = "CoinValidatorCommunicatorMonitor",
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };

            // Set poll timer to elapsed event.
            PollTimer.Elapsed += OnPollTimeout;
            PollTimer.Interval = DeviceConfig.pollingFrequency;

            _monitoringThread.Start();
            PollTimer.Start();
            return true;
        }

        /// <summary>call to stop monitoring thread for pwm device</summary>
        /// <returns>True if success. <see cref="ChangeRecord"</returns>
        protected bool StopDeviceMonitoring()
        {
            Running = false;
            _monitoringThread?.Join();
            return true;
        }

        protected abstract void Run();

        private static void OnPollTimeout(object sender, ElapsedEventArgs e)
        {
            // Set to poll.
            Poll.Set();
        }

        /// <summary>call get the device path</summary>
        /// <returns>True if success.<see cref="ChangeRecord"</returns>
        private bool GetDevice()
        {
            var _deviceInterface = DeviceConfig.DeviceInterface;

            var cr = NativeMethods.CM_Get_Device_Interface_List_Size(out uint size,
                ref _deviceInterface, null, NativeConstants.CmGetDeviceInterfaceListPresent);

            if (cr != NativeConstants.CrSuccess || size == 0)
            {
                Logger.Error($"{nameof(Device)}: Cannot find device list for {_deviceInterface}");
                return false;
            }

            cr = NativeMethods.CM_Get_Device_Interface_List(ref _deviceInterface,
                null, _device, (uint)_device.Length, NativeConstants.CmGetDeviceInterfaceListPresent);
            if (cr != NativeConstants.CrSuccess || _device.Length == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>call open the device</summary>
        /// <returns>True if success.<see cref="ChangeRecord"</returns>
        private bool OpenDevice()
        {
            Logger.Info($"{nameof(Device)}: Opening device {_device}");

            if (DeviceConfig.Mode == CreateFileOption.Overlapped)
            {
                _deviceHandle = NativeMethods.CreateFile(new string(_device),
                NativeConstants.GenericRead | NativeConstants.GenericWrite,
                NativeConstants.FileShareRead | NativeConstants.FileShareWrite,
                IntPtr.Zero,
                NativeConstants.OpenExisting,
                NativeConstants.FileFlagOverlapped,
                IntPtr.Zero);

            }
            else
            {
                _deviceHandle = NativeMethods.CreateFile(new string(_device),
                    NativeConstants.GenericRead | NativeConstants.GenericWrite,
                    NativeConstants.FileShareRead | NativeConstants.FileShareWrite,
                    IntPtr.Zero,
                    NativeConstants.OpenExisting,
                    0,
                    IntPtr.Zero);
            }
            return !_deviceHandle.IsInvalid;
        }
    }
}
