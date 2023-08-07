namespace Aristocrat.Monaco.Asp.Client.Comms
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using log4net;
    using Microsoft.Win32.SafeHandles;
    using Win32Comm;

    public class CommPort : ICommPort
    {
        /// The 19.2K baud rate to use per the Dacom spec section "1.2 Logical Interface" >
        private const int DacomBaudRate = 19_200;

        /// Number of databits 1..8 (default: 8) >
        private const int DataBits = 8;

        /// The number of milliseconds to wait for a read>
        private const int ReadWait = 20;

        /// The maximum number of bytes are saved in the Rx queue. >
        private const int RxQueueSize = 8;

        /// The maximum number of bytes are saved in the Tx queue. >
        private const int TxQueueSize = 64;

        /// A constant representing an infinite timeout value.>
        private const int InfiniteTimeout = -1;

        /// The multiplier used to calculate the total time-out period for read operations >
        private const int ReadTotalTimeoutMultiplier = InfiniteTimeout;

        /// The number of milliseconds to calculate the total time-out period for write operations >
        private const int WriteTotalTimeout = 100;

        /// The multiplier used to calculate the total time-out period for write operations >
        private const int WriteTotalTimeoutMultiplier = 10;

        /// Create a logger for use in this class >
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private bool _disposed;

        private SafeFileHandle _portHandle;

        public bool IsOpen => NativeMethods.GetHandleInformation(_portHandle, out _);
        public string PortName { get; set; }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Open()
        {
            var portDcb = new Dcb();
            var commTimeouts = new CommTimeouts
            {
                ReadIntervalTimeout = InfiniteTimeout,
                ReadTotalTimeoutConstant = ReadWait,
                ReadTotalTimeoutMultiplier = ReadTotalTimeoutMultiplier,
                WriteTotalTimeoutConstant = WriteTotalTimeout,
                WriteTotalTimeoutMultiplier = WriteTotalTimeoutMultiplier
            };

            portDcb.Init(true);
            portDcb.BaudRate = DacomBaudRate;
            portDcb.StopBits = (byte)StopBit.One;
            portDcb.Parity = (byte)Parity.None;
            portDcb.ByteSize = DataBits;
            try
            {
                _portHandle = NativeMethods.CreateFile(
                    PortName,
                    NativeMethods.GenericRead | NativeMethods.GenericWrite,
                    0,
                    IntPtr.Zero,
                    NativeMethods.OpenExisting,
                    0,
                    IntPtr.Zero);
                if (_portHandle == null || _portHandle.IsInvalid)
                {
                    var errId = Marshal.GetLastWin32Error();
                    Logger.Error($"[Asp] Failed to open the serial port on {PortName}; error code = {errId}");
                    throw new Exception();
                }

                if (!NativeMethods.SetCommState(_portHandle, ref portDcb))
                {
                    Logger.Error("[Asp] Bad com settings.");
                    throw new Exception();
                }

                if (!NativeMethods.SetupComm(_portHandle, RxQueueSize, TxQueueSize))
                {
                    Logger.Error("[Asp] Failed to setup communication.");
                    throw new Exception();
                }

                if (!NativeMethods.SetCommTimeouts(_portHandle, ref commTimeouts))
                {
                    Logger.Error("[Asp] Bad timeout settings.");
                    throw new Exception();
                }

                Purge();
            }
            catch (Exception)
            {
                Logger.Error("[Asp]");
                _portHandle?.Dispose();
                _portHandle = null;
            }
        }

        public void Purge()
        {
            NativeMethods.PurgeComm(
                _portHandle,
                NativeMethods.PurgeTxAbort | NativeMethods.PurgeRxAbort | NativeMethods.PurgeTxClear |
                NativeMethods.PurgeRxClear);
        }

        public void Close()
        {
            _portHandle?.Dispose();
            _portHandle = null;
        }

        public int Read(byte[] buffer, int offset, uint numberOfBytesToRead)
        {
            NativeMethods.FlushFileBuffers(_portHandle);
            unsafe
            {
                fixed (byte* bufferPointer = buffer)
                {
                    if (!NativeMethods.ReadFile(
                        _portHandle,
                        bufferPointer + offset,
                        numberOfBytesToRead,
                        out var numberOfBytesRead,
                        null))
                    {
                        return -1;
                    }

                    return (int)numberOfBytesRead;
                }
            }
        }

        public int Write(byte[] bytesToWrite, int offset, uint numberOfBytesToWrite)
        {
            unsafe
            {
                fixed (byte* bufferPointer = bytesToWrite)
                {
                    uint numberOfBytesWritten = 0;
                    while (numberOfBytesWritten < numberOfBytesToWrite)
                    {
                        if (!NativeMethods.WriteFile(
                            _portHandle,
                            bufferPointer + offset,
                            numberOfBytesToWrite,
                            out numberOfBytesWritten,
                            null))
                        {
                            return -1;
                        }

                        offset += (int)numberOfBytesWritten;
                    }

                    NativeMethods.FlushFileBuffers(_portHandle);
                    return (int)numberOfBytesWritten;
                }
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
                Close();
            }

            _disposed = true;
        }
    }
}