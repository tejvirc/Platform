namespace Aristocrat.Monaco.Hardware.Contracts.SerialPorts
{
    using System;
    using System.IO;
    using System.IO.Ports;
    using System.Reflection;
    using System.Timers;
    using Communicator;
    using Kernel;
    using log4net;
    using SharedDevice;

    /// <summary>
    ///     Expands on System.IO.Ports.SerialPort
    /// </summary>
    public class SerialPortController : SerialPort, ISerialPortController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Timer _keepAliveTimer;

        private ISerialPortsService _serialPortsService;

        /// <summary>
        ///     Data has been received on the port.
        /// </summary>
        public event EventHandler ReceivedData;

        /// <summary>
        ///     Error has been received on the port.
        /// </summary>
        public event EventHandler ReceivedError;

        /// <summary>
        ///     The keep-alive timer expired during a receive operation.
        /// </summary>
        public event EventHandler KeepAliveExpired;

        /// <summary>
        ///     Gets or sets a value indicating if the communication sync mode is used.
        /// </summary>
        public bool UseSyncMode { set; get; } = false;

        /// <inheritdoc />
        public void Configure(IComConfiguration comConfig)
        {
            _serialPortsService ??= ServiceManager.GetInstance().TryGetService<ISerialPortsService>();
            if (_serialPortsService is null)
            {
                Logger.Debug("Serial Port Service Not available. Probably shutting down");
                return;
            }

            PortName = _serialPortsService.LogicalToPhysicalName(comConfig.PortName);
            if (comConfig.Mode != ComConfiguration.RS232CommunicationMode)
            {
                return;
            }

            Logger.Debug($"Configure RS232 {PortName}");

            if (!UseSyncMode)
            {
                DataReceived += PortDataReceived;
                ErrorReceived += PortErrorReceived;
            }

            BaudRate = comConfig.BaudRate;
            DataBits = comConfig.DataBits;
            Parity = comConfig.Parity;
            StopBits = comConfig.StopBits;
            Handshake = comConfig.Handshake;
            ReadBufferSize = comConfig.ReadBufferSize;
            WriteBufferSize = comConfig.WriteBufferSize;
            ReadTimeout = comConfig.ReadTimeoutMs;
            WriteTimeout = comConfig.WriteTimeoutMs;

            ClearKeepAliveTimer();
            if (comConfig.KeepAliveTimeoutMs <= 0)
            {
                return;
            }

            _keepAliveTimer = new Timer { AutoReset = true };
            _keepAliveTimer.Elapsed += KeepAliveTimerElapsed;
            _keepAliveTimer.Stop();
            _keepAliveTimer.Interval = comConfig.KeepAliveTimeoutMs;
            _keepAliveTimer.Enabled = true;
        }

        private void ClearKeepAliveTimer()
        {
            if (_keepAliveTimer != null)
            {
                _keepAliveTimer.Elapsed -= KeepAliveTimerElapsed;
                _keepAliveTimer.Dispose();
            }

            _keepAliveTimer = null;
        }

        /// <inheritdoc />
        public bool IsEnabled
        {
            get => IsOpen;
            set
            {
                if (IsOpen == value)
                {
                    return;
                }

                if (value)
                {
                    Logger.Debug($"Enable RS232 {PortName}");
                    try
                    {
                        _serialPortsService.RegisterPort(PortName);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Unable to register {ex.Message}");
                        return;
                    }

                    var curHandshake = Handshake;

                    // Clear the handshake to reset the XOn/XOff state if it is active otherwise it can get stuck in XOff
                    Handshake = Handshake.None;
                    Open();
                    Handshake = curHandshake;

                    FlushInputAndOutput();
                }
                else
                {
                    Logger.Debug($"Disable RS232 {PortName}");
                    FlushInputAndOutput();

                    Close();

                    _serialPortsService.UnRegisterPort(PortName);
                }
            }
        }

        /// <inheritdoc />
        public bool WriteBuffer(byte[] buffer)
        {
            if (!IsOpen || !IsEnabled)
            {
                return false;
            }

            try
            {
                Write(buffer, 0, buffer.Length);
                return true;
            }
            catch (TimeoutException e)
            {
                Logger.Error($"{PortName} SND Timed out {BitConverter.ToString(buffer)}", e);
                return false;
            }
        }

        /// <inheritdoc />
        public int TryReadBuffer(ref byte[] buffer, int offset, int count)
        {
            if (!IsOpen || !IsEnabled || BytesToRead < count)
            {
                return 0;
            }

            if (UseSyncMode)
            {
                _keepAliveTimer?.Stop();
                _keepAliveTimer?.Start();
            }

            try
            {
                var cnt = Read(buffer, offset, count);
                return cnt;
            }
            catch (IOException e)
            {
                Logger.Debug($"Exception: {e}");
                return 0;
            }
            catch (TimeoutException e)
            {
                Logger.Error($"{PortName} RCV Timed out {BitConverter.ToString(buffer)}", e);
                return 0;
            }
        }

        /// <inheritdoc />
        public bool FlushInputAndOutput()
        {
            if (!IsOpen)
            {
                return false;
            }

            try
            {
                DiscardInBuffer();

                // Also flush the output while we're at it.
                DiscardOutBuffer();
            }
            catch (IOException e)
            {
                Logger.Debug($"Exception: {e}");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!disposing || !IsEnabled)
            {
                return;
            }

            ClearKeepAliveTimer();
            base.Dispose(true);
            _serialPortsService.UnRegisterPort(PortName);
        }

        private void PortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _keepAliveTimer?.Stop();
            _keepAliveTimer?.Start();
            ReceivedData?.Invoke(this, EventArgs.Empty);
        }

        private void PortErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            ReceivedError?.Invoke(this, EventArgs.Empty);
        }

        private void KeepAliveTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (IsEnabled)
            {
                KeepAliveExpired?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}