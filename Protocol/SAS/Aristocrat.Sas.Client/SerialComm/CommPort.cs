namespace Aristocrat.Sas.Client.SerialComm
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using log4net;
    using Win32Comm;

    /// <summary>
    ///     A class to establish a link to the backend and do read and write.
    /// </summary>
    public class CommPort : ISasCommPort, IDisposable
    {
        private const string WindowsFileSystemComPortPrefix = "\\\\.\\";

        /// <summary>Create a logger for use in this class</summary>
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary> The 19.2K baud rate to use per the SAS spec section "1.2 Logical Interface" </summary>
        private const int SasBaudRate = 19_200;

        /// <summary> Number of databits 1..8 (default: 8) </summary>
        private const int DataBits = 8;

        /// <summary> The character used to signal Xon for X flow control (default: DC1) </summary>
        /// <remarks>
        /// XOn/XOff for Sas is not used, but the Win32 comm port calls will fail
        /// if XOn/XOff have the same values.
        /// </remarks>
        private const AsciiCode XonChar = AsciiCode.DC1;

        /// <summary> The character used to signal Xoff for X flow control (default: DC3) </summary>
        /// <remarks>
        /// XOn/XOff for Sas is not used, but the Win32 comm port calls will fail
        /// if XOn/XOff have the same values.
        /// </remarks>
        private const AsciiCode XoffChar = AsciiCode.DC3;

        /// <summary> The number of free bytes in the reception queue at which flow is disabled (default: 2048)</summary>
        private const int RxHighWater = 2048;

        /// <summary> The number of bytes in the reception queue at which flow is re-enabled (default: 512)</summary>
        private const int RxLowWater = 512;

        /// <summary> The maximum number of bytes are saved in the Rx queue. </summary>
        private const int RxQueueSize = 8;

        /// <summary> The maximum number of bytes are saved in the Tx queue. </summary>
        private const int TxQueueSize = 64;

        /// <summary> A constant representing an infinite timeout value.</summary>
        private const int InfiniteTimeout = -1;

        /// <summary> The multiplier used to calculate the total time-out period for read operations </summary>
        private const int ReadTotalTimeoutMultiplier = InfiniteTimeout;

        /// <summary> The number of milliseconds to wait for a read</summary>
        private const int ReadWait = 20;

        /// <summary> The number of milliseconds to wait for an event on the port </summary>
        private const int WaitCommInterval = 1;

        /// <summary> The number of milliseconds to calculate the total time-out period for write operations </summary>
        private const int WriteTotalTimeout = 100;

        /// <summary> The multiplier used to calculate the total time-out period for write operations </summary>
        private const int WriteTotalTimeoutMultiplier = 10;

        /// <summary> The number of milliseconds to wait for a byte transmission </summary>
        private const int TransmitWait = 10;

        /// <summary> If the link remains inactive within The number of milliseconds, recheck it (default: 2000ms) </summary>
        private const int LinkRecheckWait = 2000;

        /// <summary> If the link remains inactive within The number of milliseconds, log a message (default: 30000ms) </summary>
        private const int LogLinkInactiveWait = 30000;

        /// <summary> The maximum number of times to attempt to send the data out. </summary>
        private const int MaxWriteAttempts = 5;

        private SafeFileHandle _portHandle;
        private readonly AutoResetEvent _writeEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _readEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _waitCommEvent = new AutoResetEvent(false);

        private Overlapped _managedReadOverlapped;
        private Overlapped _managedWriteOverlapped;
        private Overlapped _managedWaitCommOverlapped;
        private unsafe NativeOverlapped* _nativeReadOverlapped;
        private unsafe NativeOverlapped* _nativeWriteOverlapped;
        private unsafe NativeOverlapped* _nativeWaitCommOverlapped;

        private byte[] _rxBuf = new byte[1];
        private bool _online;
        private Stopwatch _linkWatch = new Stopwatch();
        private Stopwatch _logInactiveLinkWatch = new Stopwatch();
        private bool _waitingForRx;
        private bool _messageInQ;
        private bool _resynch;
        private bool _disposed;

        /// <inheritdoc />
        public byte SasAddress { get; set; }

        /// <inheritdoc />
        public bool Open(string port)
        {
            var portDcb = new Dcb();
            var commTimeouts = new CommTimeouts();

            if (_online)
            {
                // the caller should close it first. 
                return false;
            }

            if (!port.StartsWith(WindowsFileSystemComPortPrefix, StringComparison.InvariantCulture))
            {
                // Need so we can support com ports above 9
                port = WindowsFileSystemComPortPrefix + port;
            }

            _portHandle = NativeMethods.CreateFile(port, NativeMethods.GenericRead | NativeMethods.GenericWrite, 0, IntPtr.Zero,
                NativeMethods.OpenExisting, NativeMethods.FileFlagOverlapped | NativeMethods.FileFlagNoBuffering, IntPtr.Zero);
            if (_portHandle == null || _portHandle.IsInvalid)
            {
                var errId = Marshal.GetLastWin32Error();
                Logger.Error($"[SAS] Failed to open the serial port on {port}; error code = {errId}");
                return false;
            }

            _online = true;

            _linkWatch.Start();
            _logInactiveLinkWatch.Start();
            commTimeouts.ReadIntervalTimeout = InfiniteTimeout;
            commTimeouts.ReadTotalTimeoutConstant = ReadWait;
            commTimeouts.ReadTotalTimeoutMultiplier = ReadTotalTimeoutMultiplier;
            commTimeouts.WriteTotalTimeoutConstant = WriteTotalTimeout;
            commTimeouts.WriteTotalTimeoutMultiplier = WriteTotalTimeoutMultiplier;

            portDcb.Init(true);
            portDcb.BaudRate = SasBaudRate;
            portDcb.ByteSize = DataBits;
            portDcb.Parity = (byte)Parity.Space;
            portDcb.StopBits = (byte)StopBit.One;
            portDcb.XoffChar = (byte)XoffChar;
            portDcb.XonChar = (byte)XonChar;
            portDcb.XoffLim = RxHighWater;
            portDcb.XonLim = RxLowWater;
            if (!NativeMethods.SetCommState(_portHandle, ref portDcb))
            {
                Logger.Error("[SAS] Bad com settings.");
                InternalClose();
                return false;
            }

            if (!NativeMethods.SetupComm(_portHandle, RxQueueSize, TxQueueSize))
            {
                Logger.Error("[SAS] Failed to setup communication.");
                InternalClose();
                return false;
            }

            NativeMethods.PurgeComm(_portHandle, NativeMethods.PurgeTxAbort | NativeMethods.PurgeRxAbort
                                                 | NativeMethods.PurgeTxClear | NativeMethods.PurgeRxClear);
            if (!NativeMethods.SetCommTimeouts(_portHandle, ref commTimeouts))
            {
                Logger.Error("[SAS] Bad timeout settings.");
                InternalClose();
                return false;
            }

            #pragma warning disable CS0618
            _managedWriteOverlapped = new Overlapped(
                0,
                0,
                _writeEvent.Handle,
                null);
            _managedReadOverlapped = new Overlapped(
                0,
                0,
                _readEvent.Handle,
                null);
            _managedWaitCommOverlapped = new Overlapped(
                0,
                0,
                _waitCommEvent.Handle,
                null);
            #pragma warning restore CS0618

            unsafe
            {
                _nativeWriteOverlapped = _managedWriteOverlapped.Pack(null, null);
                _nativeReadOverlapped = _managedReadOverlapped.Pack(null, null);
                _nativeWaitCommOverlapped = _managedWaitCommOverlapped.Pack(null, null);
            }
            
            return true;
        }

        /// <inheritdoc />
        public void Close()
        {
            if (_online)
            {
                InternalClose();
            }
        }

        /// <inheritdoc />
        public bool SendRawBytes(IReadOnlyCollection<byte> bytesToSend)
        {
            Write(bytesToSend.ToArray());
            
            return true;
        }

        /// <inheritdoc />
        public (byte theByte, bool wakeupBitSet, bool readStatus) ReadOneByte(bool isLongPoll)
        {
            if (!Online)
            {
                // give the caller thread's loop a pause
                Thread.Sleep(1);
                return (0xFF, false, false);
            }

            if (_linkWatch.ElapsedMilliseconds >= LinkRecheckWait)
            {
                if (!WaitComm())
                {
                    // only send a link down log message once every 30 seconds
                    if (_logInactiveLinkWatch.ElapsedMilliseconds >= LogLinkInactiveWait)
                    {
                        Logger.Warn($"[SAS] The SAS connection has been inactive for {_linkWatch.ElapsedMilliseconds} ms.");
                        _logInactiveLinkWatch.Restart();
                    }

                    return (0xFF, false, false);
                }
            }

            uint numberOfBytesRead = 0;
            bool dataAvailable = false;
            if (!_waitingForRx)
            {
                bool readStatus;
                unsafe
                {
                    readStatus = NativeMethods.ReadFile(_portHandle, _rxBuf, 1, out numberOfBytesRead, _nativeReadOverlapped);
                }

                if (readStatus)
                {
                    dataAvailable = true;
                }
                else
                {
                    _waitingForRx = true;
                }
            }

            if (_waitingForRx)
            {
                var eventResult = NativeMethods.WaitForSingleObject(_readEvent.SafeWaitHandle, ReadWait);
                if (eventResult == NativeMethods.WaitObject0)
                {
                    unsafe
                    {
                        if (NativeMethods.GetOverlappedResult(_portHandle, _nativeReadOverlapped, out numberOfBytesRead, false))
                        {
                            dataAvailable = true;
                        }
                    }
                    _waitingForRx = false;
                }
                else if (eventResult != NativeMethods.WaitTimeout)
                {
                    _waitingForRx = false;
                }
            }

            if (dataAvailable && numberOfBytesRead != 0)
            {
                _linkWatch.Restart();
                _logInactiveLinkWatch.Restart();
                bool trafficPoll = _rxBuf[0] > SasConstants.PollBit && !isLongPoll;
                if (trafficPoll && _resynch)
                {
                    return (_rxBuf[0], false, true);
                }

                bool wakeup = false;
                var commStatus = NativeMethods.ClearCommError(_portHandle, out uint errs, out ComStat comStat);
                if (commStatus && (errs & NativeMethods.CeRxParity) != 0 && !_messageInQ)
                {
                    Logger.Info($"[SAS] Got a wakeup bit on {_rxBuf[0]:X2}.");
                    wakeup = true;
                    _resynch = false;
                }
                else if (_rxBuf[0] == SasConstants.PollBit && (errs & NativeMethods.CeRxParity) == 0 && _messageInQ && !isLongPoll)
                {
                    Logger.Info("[SAS] Handled 0x80 with no parity");
                    _resynch = true;
                }

                _messageInQ = comStat.cbInQue > 0;
                return (_rxBuf[0], wakeup, true);
            }

            return (0xFF, false, false);
        }

        /// <inheritdoc/>
        public bool SendChirp()
        {
            if (!Online)
            {
                return false;
            }

            Logger.Info("[SAS] Chirping ......");
            ChangeParity(Parity.Mark);
            WriteOneByte(SasAddress);
            Thread.Sleep(TransmitWait);
            ChangeParity(Parity.Space);

            NativeMethods.ClearCommError(_portHandle, out _, out _);

            return true;
        }

        private bool Online => _online && NativeMethods.GetHandleInformation(_portHandle, out _);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        private void InternalClose()
        {
            if (!_online)
            {
                return;
            }

            _writeEvent.Close();
            _readEvent.Close();
            _waitCommEvent.Close();
            _portHandle.Close();

            unsafe
            {
                Overlapped.Unpack(_nativeReadOverlapped);
                Overlapped.Free(_nativeReadOverlapped);
                Overlapped.Unpack(_nativeWriteOverlapped);
                Overlapped.Free(_nativeWriteOverlapped);
                Overlapped.Unpack(_nativeWaitCommOverlapped);
                Overlapped.Free(_nativeWaitCommOverlapped);
            }

            _online = false;
        }

        private void Write(IReadOnlyCollection<byte> bytesToSend)
        {
            if (!Online)
            {
                return;
            }

            var bytesWritten = 0;
            var attempts = 0;
            var len = bytesToSend.Count;
            while (bytesWritten < len && attempts++ < MaxWriteAttempts)
            {
                bool writeStatus;
                unsafe
                {
                    writeStatus = NativeMethods.WriteFile(_portHandle, bytesToSend.ToList().GetRange(bytesWritten, len - bytesWritten).ToArray(),
                    (uint)(len - bytesWritten), out _, _nativeWriteOverlapped);
                }

                if (!writeStatus)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != NativeMethods.ErrorIoPending)
                    {                        
                        Logger.Error($"[SAS] WriteFile: unexpected failure; error code = {error}");
                        break;
                    }

                    uint sent;
                    unsafe
                    {
                        NativeMethods.GetOverlappedResult(_portHandle, _nativeWriteOverlapped, out sent, true);
                    }

                    bytesWritten += (int)sent;
                }
                else
                {
                    break;
                }
            }

            if (bytesWritten > len)
            {
                // it's impossible in general but still need to check.
                Logger.Error($"[SAS] {len} bytes are expected to be sent but actual {bytesWritten} bytes have been sent.");
            }
        }

        /// <summary>
        ///     Sends a protocol byte immediately ahead of any queued bytes.
        /// </summary>
        /// <param name="tosend">Byte to send</param>
        /// <returns>False if an immediate byte is already scheduled and not yet sent</returns>
        private void WriteOneByte(byte tosend)
        {
            if (!NativeMethods.TransmitCommChar(_portHandle, tosend))
            {
                Logger.Error("[SAS] Transmission failure");
            }
        }

        /// <summary>
        ///     Waits for any byte or error on the connection to be available within a specified wait period.
        /// </summary>
        /// <returns>
        ///     true - if there is a byte or an error on the connection
        ///     false - no event is triggered within a specified wait period
        /// </returns>
        private bool WaitComm()
        {
            bool status;
            if (!NativeMethods.SetCommMask(_portHandle, NativeMethods.EvRxChar | NativeMethods.EvErr))
            {
                // it won't happen when the connection is healthy. 
                Logger.Error($"[SAS] IO Error {Marshal.GetLastWin32Error()}");
                return false;
            }

            unsafe
            {
                uint mask = 0;
                status = NativeMethods.WaitCommEvent(_portHandle, new IntPtr(&mask), _nativeWaitCommOverlapped);
            }

            if (!status)
            {
                status = _waitCommEvent.WaitOne(WaitCommInterval);
            }

            return status;
        }

        private void ChangeParity(Parity parity)
        {
            NativeMethods.CancelIo(_portHandle);

            var dcb = new Dcb();
            if (!NativeMethods.GetCommState(_portHandle, ref dcb))
            {
                Logger.Error($"[SAS] GetCommState: I/O error {Marshal.GetLastWin32Error()}.");
                return;
            }

            dcb.Parity = (byte)parity;
            if (!NativeMethods.SetCommState(_portHandle, ref dcb))
            {
                Logger.Error($"[SAS] SetCommState: I/O error {Marshal.GetLastWin32Error()}.");
            }
        }
    }
}
