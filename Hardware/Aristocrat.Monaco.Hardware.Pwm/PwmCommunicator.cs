namespace Aristocrat.Monaco.Hardware.Pwm
{
    using Aristocrat.Monaco.Hardware.Contracts.Communicator;
    using Aristocrat.Monaco.Hardware.Contracts.Gds;
    using Aristocrat.Monaco.Hardware.Contracts.PWM;
    using Aristocrat.Monaco.Hardware.Contracts.SharedDevice;
    using log4net;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
 

    /// <summary>
    /// 
    /// </summary>
    public abstract class PwmCommunicator: IGdsCommunicator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private char[] device_ = new char[256];
        private System.Threading.NativeOverlapped _overlapped;
        private SafeFileHandle _deviceHandle;
        protected IntPtr _dataPtr { get; set; }
        protected PwmDeviceConfig DeviceConfig { get; set; }

        protected bool _waitingForRx;
        protected bool _disposed;


        // TBD just divider

        public PwmCommunicator()
        {
            _overlapped = new System.Threading.NativeOverlapped()
            {
                OffsetLow = 0,
                OffsetHigh = 0,
                EventHandle = NativeMethods.CreateEvent(IntPtr.Zero, Convert.ToInt32(false), Convert.ToInt32(false), "")
            };
            _dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ChangeRecord)));
            _waitingForRx = false;
            _disposed = false;
        }

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
                null, device_, (uint)device_.Length, NativeConstants.CmGetDeviceInterfaceListPresent);
            if (cr != NativeConstants.CrSuccess || device_.Length == 0)
            {
                return false;
            }

            return true;
        }

        private bool OpenDevice()
        {
            Logger.Info($"{nameof(Device)}: Opening device {device_}");

            if (DeviceConfig.Mode == CreateFileOption.Overlapped)
            {
                _deviceHandle = NativeMethods.CreateFile(new string(device_),
                NativeConstants.GenericRead | NativeConstants.GenericWrite,
                NativeConstants.FileShareRead | NativeConstants.FileShareWrite,
                IntPtr.Zero,
                NativeConstants.OpenExisting,
                NativeConstants.FileFlagOverlapped,
                IntPtr.Zero);

            }
            else
            {
                _deviceHandle = NativeMethods.CreateFile(new string(device_),
                    NativeConstants.GenericRead | NativeConstants.GenericWrite,
                    NativeConstants.FileShareRead | NativeConstants.FileShareWrite,
                    IntPtr.Zero,
                    NativeConstants.OpenExisting,
                    0,
                    IntPtr.Zero);
            }

            return _deviceHandle.IsInvalid;

        }

        public bool Open()
        {
            if(GetDevice())
            {
               if(OpenDevice())
                {
                    IsOpen =  StartDeviceMonitoring();
                }
            }
            return IsOpen;
        }


        public virtual (bool, ChangeRecord) ReadSync()
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
                  out var bytesRead,
                  IntPtr.Zero);

                if (success)
                {
                    ChangeRecord record = (ChangeRecord)Marshal.PtrToStructure(_dataPtr, typeof(ChangeRecord));
                    Marshal.FreeHGlobal(_dataPtr);
                    _dataPtr = Marshal.AllocHGlobal(size);
                    return (true, record);
                }
            }
            return (false, new ChangeRecord());
        }

        public virtual (bool, ChangeRecord) ReadAsync()
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
                    ChangeRecord record = (ChangeRecord)Marshal.PtrToStructure(_dataPtr, typeof(ChangeRecord));
                    Marshal.FreeHGlobal(_dataPtr);
                    _dataPtr = Marshal.AllocHGlobal(size);
                    return (true, record);
                }
            }
            return (false, new ChangeRecord());
        }
        public virtual (bool, ChangeRecord) Read()
        {

            return DeviceConfig.Mode == CreateFileOption.Overlapped ? ReadAsync() : ReadSync();
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
        public bool Ioctl<T, C>(C command, T data) where C : struct, IConvertible
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
        public bool IoctlAsync<T, C, R>(C command, T data, ref R outData) where C : struct, IConvertible
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
                NativeConstants.COINACCEPTOR_MAKE_IOCTL(DeviceConfig.DeviceType, CoinAcceptorCommands.CoinAcceptorPeek),
                IntPtr.Zero,
                0,
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
        public bool Ioctl<T, C, R>(C command, T data, ref R outData) where C : struct, IConvertible
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

        //-------------

        public virtual string Manufacturer { get; set; }

        public virtual string Model { get; set; }

        public string Firmware => string.Empty;

        public string SerialNumber => string.Empty;

        public bool IsOpen { get; set; }

        public int VendorId { get; set; } = -1;

        public int ProductId { get; set; } = -1;

        public int ProductIdDfu { get; set; } = -1;

        public string Protocol => "Pwm";

        public DeviceType DeviceType { get; set; }
        public IDevice Device { get; set; }

        public string FirmwareVersion => string.Empty;

        public string FirmwareRevision => string.Empty;

        public int FirmwareCrc => -1;

        public string BootVersion => string.Empty;

        public string VariantName => string.Empty;

        public string VariantVersion => string.Empty;

        public bool IsDfuCapable => false;

        public event EventHandler<EventArgs> DeviceAttached;
        public event EventHandler<EventArgs> DeviceDetached;
        public event EventHandler<GdsSerializableMessage> MessageReceived;

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


        public void ResetConnection()
        {
            throw new NotImplementedException();
        }


        protected abstract bool StartDeviceMonitoring();
        protected abstract bool StopDeviceMonitoring();
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract bool Configure(IComConfiguration comConfiguration);
        public abstract void SendMessage(GdsSerializableMessage message, CancellationToken token);
    }
}
