namespace Aristocrat.Monaco.Hardware.PWM
{
    using Hardware.Contracts.PWM;
    using log4net;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    /// <summary>A PWM device adapter.</summary>
    public abstract class PwmDevice<Implementation> where Implementation : IPwmDevice
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private char[] device_ = new char[256];
        protected abstract IPwmDevice Device { get; }
        protected System.Threading.NativeOverlapped _overlapped;
        protected SafeFileHandle _deviceHandle;
        protected IntPtr _dataPtr { get; set; }
        protected bool _waitingForRx;
        protected bool _disposed;

        public PwmDevice()
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
        protected bool GetDevice()
        {
            var _deviceInterface = Device.DeviceConfig.DeviceInterface;

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

        protected bool OpenDevice()
        {
            Logger.Info($"{nameof(Device)}: Opening device {device_}");

            if (Device.DeviceConfig.Mode == CreateFileOption.Overlapped)
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

            if (Device.DeviceConfig.Mode != CreateFileOption.Overlapped)
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
                    var result = NativeMethods.WaitForSingleObject(_overlapped.EventHandle, Device.DeviceConfig.waitPeriod);

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

            return Device.DeviceConfig.Mode == CreateFileOption.Overlapped ? ReadAsync() : ReadSync();
        }

        public virtual void Cleanup()
        {
            if (_deviceHandle == null)
            {
                return;
            }

            if (_deviceHandle.IsInvalid)
            {
                return;
            }
            NativeMethods.CancelIoEx(_deviceHandle, IntPtr.Zero);
            _deviceHandle.Close();
            _deviceHandle.SetHandleAsInvalid();
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

            Cleanup();

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
                    NativeConstants.PWMDEVICE_MAKE_IOCTL(Device.DeviceConfig.DeviceType, command),
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
            if (Device.DeviceConfig.Mode != CreateFileOption.Overlapped)
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
                NativeConstants.COINACCEPTOR_MAKE_IOCTL(Device.DeviceConfig.DeviceType, CoinAcceptorCommands.CoinAcceptorPeek),
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
                    var result = NativeMethods.WaitForSingleObject(_overlapped.EventHandle, Device.DeviceConfig.waitPeriod);

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
                    NativeConstants.PWMDEVICE_MAKE_IOCTL(Device.DeviceConfig.DeviceType, command),
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
    }
}
