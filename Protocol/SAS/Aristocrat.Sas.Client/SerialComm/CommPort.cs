namespace Aristocrat.Sas.Client.SerialComm
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using log4net;
    using Monaco.NativeSerial;

    /// <summary>
    ///     A class to establish a link to the backend and do read and write.
    /// </summary>
    public class CommPort : ISasCommPort, IDisposable
    {
        /// <summary>Create a logger for use in this class</summary>
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

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

        /// <summary> The number of milliseconds to wait for a read</summary>
        private const int ReadWait = 20;

        /// <summary> The number of milliseconds to calculate the total time-out period for write operations </summary>
        private const int WriteTotalTimeout = 100;

        /// <summary> If the link remains inactive within The number of milliseconds, log a message (default: 30000ms) </summary>
        private const int LogLinkInactiveWait = 30000;

        /// <summary> The maximum number of times to attempt to send the data out. </summary>
        private const int MaxWriteAttempts = 5;

        private readonly INativeComPort _nativeCom;

        private readonly Stopwatch _linkWatch = new();
        private readonly Stopwatch _logInactiveLinkWatch = new();
        private bool _disposed;
        private bool _resynch;
        private bool _messageInQ;

        /// <inheritdoc />
        public byte SasAddress { get; set; }

        /// <summary>
        ///     Creates an instance of <see cref="CommPort"/>
        /// </summary>
        /// <param name="nativeCom">An instance of <see cref="INativeComPort"/></param>
        public CommPort(INativeComPort nativeCom)
        {
            _nativeCom = nativeCom ?? throw new ArgumentNullException(nameof(nativeCom));
        }

        /// <summary>
        ///     Creates an instance of <see cref="CommPort"/>
        /// </summary>
        public CommPort() : this(NativeSerialPortFactory.CreateSerialPort())
        {
        }

        /// <inheritdoc />
        public bool Open(string port)
        {
            return _nativeCom.Open(port, new SerialConfiguration
            {
                BaudRate = SasBaudRate,
                BitsPerByte = DataBits,
                Parity = System.IO.Ports.Parity.Space,
                ReadIntervalTimeout = Timeout.InfiniteTimeSpan,
                ReadTotalTimeout = TimeSpan.FromMilliseconds(ReadWait),
                RxHighWater = RxHighWater,
                RxLowWater = RxLowWater,
                RxQueueSize = RxQueueSize,
                StopBits = NativeStopBits.One,
                TxQueueSize = TxQueueSize,
                WriteTotalTimeout = TimeSpan.FromMilliseconds(WriteTotalTimeout),
                XOffChar = (byte)XoffChar,
                XOnChar = (byte)XonChar
            });
        }

        /// <inheritdoc />
        public void Close()
        {
            _nativeCom.Close();
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
            if (!_nativeCom.IsOpen)
            {
                return (0xFF, false, false);
            }

            var comPortByte = _nativeCom.Read();
            if (comPortByte.Status)
            {
                _logInactiveLinkWatch.Restart();
                var trafficPoll = comPortByte.Data > SasConstants.PollBit && !isLongPoll;
                if (trafficPoll && _resynch)
                {
                    return (comPortByte.Data, false, true);
                }

                var wakeup = false;
                if ((comPortByte.ComErrors & ComPortErrors.Parity) != 0 && !_messageInQ)
                {
                    Logger.Info($"[SAS] Got a wakeup bit on {comPortByte.Data:X2}.");
                    wakeup = true;
                    _resynch = false;
                }
                else if (comPortByte.Data == SasConstants.PollBit && (comPortByte.ComErrors & ComPortErrors.Parity) == 0 && _messageInQ && !isLongPoll)
                {
                    Logger.Info("[SAS] Handled 0x80 with no parity");
                    _resynch = true;
                }

                _messageInQ = comPortByte.ByteInQueue > 0;
                return (comPortByte.Data, wakeup, true);
            }

            if (_logInactiveLinkWatch.ElapsedMilliseconds < LogLinkInactiveWait)
            {
                return (comPortByte.Data, (comPortByte.ComErrors & ComPortErrors.Parity) != 0, comPortByte.Status);
            }

            Logger.Warn($"[SAS] The SAS connection has been inactive for {_linkWatch.ElapsedMilliseconds} ms.");
            _logInactiveLinkWatch.Restart();
            return (comPortByte.Data, (comPortByte.ComErrors & ComPortErrors.Parity) != 0, comPortByte.Status);
        }

        /// <inheritdoc/>
        public bool SendChirp()
        {
            return _nativeCom.WriteWithModifyParity(SasAddress, System.IO.Ports.Parity.Mark) == 1;
        }

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
                _nativeCom.Dispose();
            }

            _disposed = true;
        }

        private void Write(byte[] bytesToSend)
        {
            var bytesWritten = 0;
            var attempts = 0;
            var len = bytesToSend.Length;
            while (bytesWritten < len && attempts++ < MaxWriteAttempts)
            {
                bytesWritten += _nativeCom.Write(bytesToSend, bytesWritten, len - bytesWritten);
            }

            if (bytesWritten > len)
            {
                // it's impossible in general but still need to check.
                Logger.Error($"[SAS] {len} bytes are expected to be sent but actual {bytesWritten} bytes have been sent.");
            }
        }
    }
}
