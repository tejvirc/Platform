namespace Aristocrat.Monaco.Asp.Client.Comms
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using log4net;

    /// <summary>
    ///     Asp protocol data link layer. Responsible for ensuring the connection and data integrity.
    /// </summary>
    public class DataLinkLayer : IAspClient, IDisposable
    {
        // Time taken to receive a complete packet. Must be 350 ms (Worst Case Transmit Time)
        private const int WcttThreshold = 350;

        // Response Timeout 
        private const int ResponseTimeout = 200;

        // Time spend during poll-wait before going to link down
        private const int PollWaitTimeoutThreshold = 3 * ResponseTimeout + WcttThreshold;

        // Time spend during link-down before going to poll wait
        private const int LinkDownTimeoutThreshold = 4 * ResponseTimeout + WcttThreshold;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly DataLinkPacket _ackResp = new DataLinkPacket();
        private readonly Stopwatch _linkTimeout = new Stopwatch();
        private readonly object _syncObject = new object();
        private int _currentSequence = -1;
        private bool _disposed;
        private bool _isLinkUp;
        private volatile bool _stop;
        private Thread _thread;

        public DataLinkLayer(ICommPort port)
        {
            Port = port;
        }

        private ICommPort Port { get; set; }
        public bool IsRunning => _thread?.IsAlive ?? false;

        public bool IsLinkUp
        {
            get => _isLinkUp;
            private set
            {
                if (_isLinkUp == value)
                {
                    return;
                }

                _isLinkUp = value;
                Logger.Debug("Asp on link status changed " + _isLinkUp);
                OnLinkStatusChanged();
            }
        }

        public bool Start(string portName)
        {
            lock (_syncObject)
            {
                Stop();
                _thread = new Thread(CommsTask) { Name = "AspDataLinkTask" };
                _stop = false;
                if (!OpenPort(portName))
                {
                    return false;
                }

                _thread.Start();
            }

            return true;
        }

        public void Stop()
        {
            lock (_syncObject)
            {
                _stop = true;

                if (_thread == null)
                {
                    return;
                }

                if (_thread != Thread.CurrentThread)
                {
                    _thread.Join(5000);
                    if (IsRunning)
                    {
                        Logger.Error("Asp could not stop the AspDataLinkTask aborting.");
                        while (IsRunning)
                        {
                            Thread.Sleep(0);
                        }
                    }
                }

                ClosePort();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Stop();
                Port?.Dispose();
            }

            Port = null;
            _disposed = true;
        }

        protected virtual void OnLinkStatusChanged()
        {
            Logger.Debug("OnLinkStatusChanged");
        }

        private void ClosePort()
        {
            Port?.Close();
        }

        private bool OpenPort(string portName)
        {
            ClosePort();
            if (Port == null)
            {
                Logger.Error("Asp port should not be null.");
                return false;
            }

            Port.PortName = portName;
            Port.Open();
            return Port.IsOpen;
        }

        private void CommsTask()
        {
            var packet = new DataLinkPacket();
            _linkTimeout.Restart();
            while (!_stop)
            {
                CheckForLinkTimeout();
                try
                {
                    var readSize = Port.Read(packet.Buffer.Data, packet.Position, (uint)packet.NumberOfBytesToRead);
                    if (packet.CheckIfComplete(readSize, _currentSequence))
                    {
                        Log(packet.Bytes, 0, packet.Length, "RX");
                        _currentSequence = packet.Sequence;
                        var resp = ProcessPacketInternal(packet);
                        SendData(resp);
                        packet.Reset();
                        IsLinkUp = true;
                        _linkTimeout.Restart();
                    }
                }
                catch (TimeoutException)
                {
                    Logger.Warn("Asp communication timeout.");
                }
            }
        }

        private void CheckForLinkTimeout()
        {
            if (_linkTimeout.ElapsedMilliseconds > PollWaitTimeoutThreshold)
            {
                _currentSequence = -1;
                IsLinkUp = false;
                // Wait for LinkDownTimeoutThreshold before restarting communication.
                Thread.Sleep(LinkDownTimeoutThreshold);
                Port.Purge();
                _linkTimeout.Restart();
            }
            else if (!IsLinkUp) // If already link down then reset the timer.
            {
                _linkTimeout.Restart();
            }
        }

        private static void Log(byte[] bytes, int offset, int count, string tag)
        {
            Logger.Debug(tag + ":" + BitConverter.ToString(bytes, offset, count));
        }

        protected virtual DataLinkPacket ProcessPacket(DataLinkPacket packet)
        {
            Logger.Error("Asp base process packet should not be called.");
            return null;
        }

        private DataLinkPacket ProcessPacketInternal(DataLinkPacket packet)
        {
            var responsePacket = ProcessPacket(packet) ?? _ackResp;

            responsePacket.UpdateCrc(packet.Sequence);
            return responsePacket;
        }

        private void SendData(DataLinkPacket response)
        {
            response.UpdateCrc(_currentSequence);
            Port.Write(response.Bytes, 0, (uint)response.Length);
            Log(response.Bytes, 0, response.Length, "TX");
        }
    }
}