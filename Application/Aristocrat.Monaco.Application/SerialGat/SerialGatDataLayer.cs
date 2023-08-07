namespace Aristocrat.Monaco.Application.SerialGat
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Reflection;
    using System.Text;
    using Hardware.Contracts.Communicator;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.SerialPorts;
    using Contracts;
    using log4net;

    /// <summary>
    /// Data layer implementation for Serial GAT.
    /// </summary>
    public class SerialGatDataLayer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ISerialPortController _physicalLayer;
        private readonly Dictionary<GatQuery, (int min, int max)> _lengthRangePerQuery = new Dictionary<GatQuery, (int min, int max)>();
        private readonly Queue<GatMessage> _receivedMessages = new Queue<GatMessage>();

        private const int MaxBufferLength = 256;
        private const int CommunicationTimeoutMs = 1000;

        private bool _disposed;

        private GatMessage _messageUnderConstruction;

        /// <summary>
        ///     Event when incoming data has been received
        /// </summary>
        public event EventHandler<GatMessageReceivedEventArgs> GatMessageReceived;

        /// <summary>
        ///     Event when serial port has an error.
        /// </summary>
        public event EventHandler GatReceiveError;

        /// <summary>
        ///     Event when serial driver keep-alive expires.
        /// </summary>
        public event EventHandler GatKeepAliveExpired;

        public SerialGatDataLayer(ApplicationConfigurationGatSerial config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _lengthRangePerQuery.Add(GatQuery.Status, (min: 4, max: 4));
            _lengthRangePerQuery.Add(GatQuery.LastAuthenticationStatus, (min: 4, max: 4));
            _lengthRangePerQuery.Add(GatQuery.LastAuthenticationResults, (min: 7, max: 7));
            _lengthRangePerQuery.Add(GatQuery.InitiateAuthenticationCalculation, (min: 5, max: 255));

            _physicalLayer = new SerialPortController();
            _physicalLayer.Configure(
                new ComConfiguration
                {
                    PortName = config.ComPort,
                    Mode = ComConfiguration.RS232CommunicationMode,
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadBufferSize = MaxBufferLength,
                    WriteBufferSize = MaxBufferLength,
                    ReadTimeoutMs = SerialPort.InfiniteTimeout,
                    WriteTimeoutMs = CommunicationTimeoutMs,
                    KeepAliveTimeoutMs = CommunicationTimeoutMs
                });

            _physicalLayer.ReceivedData += PhysicalLayerDataReceived;
            _physicalLayer.ReceivedError += PhysicalLayerErrorReceived;
            _physicalLayer.KeepAliveExpired += PhysicalLayerKeepAliveExpired;
        }

        public void Enable()
        {
            _messageUnderConstruction = new GatMessage();
            _physicalLayer.IsEnabled = true;
        }

        public void Disable()
        {
            _receivedMessages.Clear();
            _physicalLayer.IsEnabled = false;
        }

        public bool IsEnabled => _physicalLayer.IsEnabled;

        public bool SendResponse(GatResponse response, byte[] packet)
        {
            if (IsEnabled)
            {
                var len = packet.Length + 4;
                var buf = new byte[len];

                var crc16 = new SerialGatCrc16();

                buf[0] = (byte)response;
                buf[1] = (byte)len;
                Buffer.BlockCopy(packet, 0, buf, 2, packet.Length);

                crc16.Hash(buf, 0, len - 2);

                buf[len - 2] = (byte)(crc16.Crc >> 8);
                buf[len - 1] = (byte)crc16.Crc;

                if (packet.Length > 0)
                {
                    Logger.Debug($"Packet out [{Encoding.ASCII.GetString(packet)}]");
                }
                return _physicalLayer.WriteBuffer(buf);
            }

            return false;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
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
                Disable();
            }

            _disposed = true;
        }

        private void ConstructMessage()
        {
            try
            {
                // First we try to get a valid message header
                if (_messageUnderConstruction.GatQuery == GatQuery.Unknown)
                {
                    var buf = new byte[2];
                    if (_physicalLayer.TryReadBuffer(ref buf, 0, 2) < 2)
                    {
                        return;
                    }

                    // first byte coming in will be the command
                    // second byte is length of total message
                    GatQuery query = (GatQuery)buf[0];
                    int len = buf[1];
                    if (_lengthRangePerQuery.ContainsKey(query))
                    {
                        var lengths = _lengthRangePerQuery[query];
                        if (len >= lengths.min && len <= lengths.max)
                        {
                            _messageUnderConstruction.GatQuery = query;
                            _messageUnderConstruction.Packet = new byte[len - 4];
                            _messageUnderConstruction.Count = 0;
                        }
                    }
                    else
                    {
                        _physicalLayer.FlushInputAndOutput();
                        return;
                    }
                }

                // Once we have a header, fill in the data packet (if any)
                if (_messageUnderConstruction.GatQuery != GatQuery.Unknown)
                {
                    var mostToRead = _messageUnderConstruction.Packet.Length - _messageUnderConstruction.Count;
                    var temp = new byte[mostToRead];
                    var cnt = _physicalLayer.TryReadBuffer(ref temp, 0, mostToRead);
                    if (cnt < mostToRead)
                    {
                        return;
                    }

                    Buffer.BlockCopy(temp, 0, _messageUnderConstruction.Packet, _messageUnderConstruction.Count, cnt);
                    _messageUnderConstruction.Count += cnt;
                }

                // CRC bytes?
                if (_messageUnderConstruction.GatQuery != GatQuery.Unknown &&
                    _messageUnderConstruction.Count == _messageUnderConstruction.Packet.Length)
                {
                    // get the CRC and finish the message processing
                    var crcBuf = new byte[2];
                    if (_physicalLayer.TryReadBuffer(ref crcBuf, 0, 2) < 2)
                    {
                        return;
                    }

                    ushort candidate = crcBuf[1];
                    candidate += (ushort)(crcBuf[0] << 8);

                    var crc16 = new SerialGatCrc16();
                    crc16.HashByte((byte)_messageUnderConstruction.GatQuery);
                    crc16.HashByte((byte)(_messageUnderConstruction.Packet.Length + 4));
                    crc16.Hash(_messageUnderConstruction.Packet, 0, _messageUnderConstruction.Packet.Length);

                    if (candidate == crc16.Crc)
                    {
                        // Good message, so send it along and start over
                        if (_messageUnderConstruction.Packet.Length > 0)
                        {
                            Logger.Debug($"Packet in [{Encoding.ASCII.GetString(_messageUnderConstruction.Packet)}]");
                        }
                        _receivedMessages.Enqueue(_messageUnderConstruction);
                    }

                    _messageUnderConstruction = new GatMessage();
                    _physicalLayer.FlushInputAndOutput();
                }
            }
            catch (Exception)
            {
                GatReceiveError?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PhysicalLayerDataReceived(object sender, EventArgs e)
        {
            if (IsEnabled)
            {
                ConstructMessage();

                if (_receivedMessages.Count > 0)
                {
                    var msg = _receivedMessages.Dequeue();
                    var evt = new GatMessageReceivedEventArgs(msg.GatQuery, msg.Packet);
                    GatMessageReceived?.Invoke(this, evt);
                }
            }
        }

        private void PhysicalLayerErrorReceived(object sender, EventArgs e)
        {
            GatReceiveError?.Invoke(this, EventArgs.Empty);
        }

        private void PhysicalLayerKeepAliveExpired(object sender, EventArgs e)
        {
            GatKeepAliveExpired?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Support class for a GAT message
        /// </summary>
        private class GatMessage
        {
            /// <summary>
            ///     Construct a new <see cref="GatMessage"/>
            /// </summary>
            public GatMessage()
            {
                GatQuery = GatQuery.Unknown;
                Count = 0;
            }

            /// <summary>
            ///     Type of message
            /// </summary>
            public GatQuery GatQuery { get; set; }

            /// <summary>
            ///     Payload data
            /// </summary>
            public byte[] Packet { get; set; }

            /// <summary>
            ///     How much of Packet has been received so far
            /// </summary>
            public int Count { get; set; }
        }
    }

    /// <summary>
    ///     Event args for GatMessage received.
    /// </summary>
    public class GatMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="query">Query type</param>
        /// <param name="packet">Support data packet</param>
        public GatMessageReceivedEventArgs(GatQuery query, byte[] packet)
        {
            GatQuery = query;
            Packet = packet;
        }

        /// <summary>
        ///     Get the query type
        /// </summary>
        public GatQuery GatQuery { get; }

        /// <summary>
        ///     Get the data packet
        /// </summary>
        public byte[] Packet { get; }
    }
}
