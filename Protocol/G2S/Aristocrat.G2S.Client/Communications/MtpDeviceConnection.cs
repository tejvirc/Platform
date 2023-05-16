namespace Aristocrat.G2S.Client.Communications
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Protocol.Common;
    using Aristocrat.Monaco.Protocol.Common.Communication;
    using log4net;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Reflection;

    /// <summary>
    ///     The implementation of a single MTP connection instance.
    /// </summary>
    public class MtpDeviceConnection : IMulticastCoordinator, IMtpDeviceConnection, IDisposable
    {
        private IPEndPoint _multicastAddress;
        private string _multicastId;
        private int _deviceId;
        private int _communicationId;
        private string _deviceClass;
        private IMessageConsumer _consumer;
        private readonly MessageBuilder _messageBuilder;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private UdpConnection _connection;
        private bool _disposed;
        private int _consecutiveHashFailures;
        private int _consecutiveMessageIdFailures;
        private Stopwatch _coordinationStopwatch = new Stopwatch();
        private const long MIN_MS_BETWEEN_COORDINATIONS = 30000;

        /// <summary>
        ///     Constructor
        /// </summary>
        public MtpDeviceConnection(string multicastId, int deviceId, int communicationId, string deviceClass, MessageBuilder messageBuilder, IPEndPoint address, byte[] currentKey, long currentMsgId, byte[] newKey, long keyChangeId, long lastMessageId)
        {
            _multicastId = multicastId ?? throw new ArgumentNullException(nameof(multicastId));
            _deviceId = deviceId;
            _communicationId = communicationId;
            _deviceClass = deviceClass;
            _messageBuilder = messageBuilder ?? throw new ArgumentNullException(nameof(messageBuilder));
            _multicastAddress = address ?? throw new ArgumentNullException(nameof(address));
            SetMtpSecurityParameters(currentKey, currentMsgId, newKey, keyChangeId, lastMessageId);
        }

        /// <inheritdoc />
        public IPEndPoint MulticastAddress
        {
            get { return _multicastAddress; }
        }

        /// <inheritdoc />
        public string MulticastId
        {
            get { return _multicastId; }
        }

        /// <inheritdoc />
        public int DeviceId
        {
            get { return _deviceId; }
        }

        /// <inheritdoc />
        public int CommunicationId
        {
            get { return _communicationId; }
        }

        /// <inheritdoc />
        public string DeviceClass
        {
            get { return _deviceClass; }
        }

        /// <inheritdoc />
        public bool Connected
        { 
            get
            {
                return (_connection?.Connected == true);
            }
        }

        /// <inheritdoc />
        public byte[] Key { get; set; }

        /// <inheritdoc />
        public long MessageId { get; set; }

        /// <inheritdoc />
        public long LastMessageId { get; set; }

        /// <inheritdoc />
        public byte[] NextKey { get; set; }

        /// <inheritdoc />
        public long KeyChangeId { get; set; }

        /// <inheritdoc />
        public bool IsEncrypted { get; set; }

        /// <inheritdoc />
        public void Open()
        {
            if (_multicastAddress == null)
            {
                Logger.Error($"No multicast address is set for MulticastId {_multicastId}");
                return;
            }

            if (_connection != null)
                _connection.Dispose();

            _connection = new UdpConnection();
            var success = _connection.Open(MulticastAddress);

            if (success.Result)
            {
                _connection.IncomingBytes.Subscribe(OnMtpPacketReceived,
                error => Logger.Error($"Error occurred while trying to receive MTP message from MulticastId {_multicastId} : {error}."));
                Logger.Info($"Connected to MulticastId {_multicastId}.");
                _disposed = false;
            }
            else
            {
                Logger.Error($"Unable to connect to MulticastId {_multicastId}.");
            }
        }

        /// <inheritdoc />
        public void Close()
        {
            Logger.Info($"Connection to MulticastId {_multicastId} closed.");
            _connection?.Close();
        }

        /// <inheritdoc />
        public void ConnectConsumer(IMessageConsumer consumer)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
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

            if (_connection != null)
                _connection.Dispose();

            _disposed = true;
        }

        /// <inheritdoc />
        public void SetMtpSecurityParameters(byte[] key, long messageId, byte[] nextKey, long keyChangeId, long lastMessageId)
        {
            Key = key?.ToArray();
            LastMessageId = lastMessageId;
            MessageId = messageId;
            NextKey = nextKey?.ToArray();
            KeyChangeId = keyChangeId;
            IsEncrypted = true;
        }

        /// <inheritdoc />
        public void SetMulticastAddress(string address)
        {
            _multicastAddress = EndpointUtilities.CreateIPEndPoint(address);
        }

        /// <inheritdoc />
        private void OnMtpPacketReceived(Packet packet)
        {
            var decryptedMessage = "";

            try
            {
                decryptedMessage = _messageBuilder.DecodeDatagram(packet.Data, this);
            }
            catch (MtpHashException ex)
            {
                Logger.Error($"MTP hash failed validation from MulticastId {_multicastId} - ", ex);
                _consecutiveHashFailures++;
            }
            catch (MtpMessageIdException ex)
            {
                Logger.Error($"Out of order MTP message ID received from MulticastId {_multicastId} - ", ex);
                _consecutiveMessageIdFailures++;
            }
            catch (Exception ex)
            {
                Logger.Error("Invalid MTP packet received from MulticastId {_multicastId} - ", ex);
            }

            if (decryptedMessage.Length > 0) // The decryption above was successful
            {
                _consecutiveHashFailures = 0;
                _consecutiveMessageIdFailures = 0;

                try
                {
                    var deserializedMessage = _messageBuilder.DecodeMessage(decryptedMessage);
                    var multicast = deserializedMessage?.Item as IBroadcast;
                    if (multicast == null)
                    {
                        Logger.Error($"Invalid multicast message received from MulticastId {_multicastId}.");
                        return;
                    }

                    if (_consumer == null)
                    {
                        Logger.Error($"No MTP message consumers attached for MulticastId {_multicastId}.");
                        return;
                    }

                    var error = _consumer.Consumes(multicast, _communicationId);

                    if (error.IsError)
                    {
                        Logger.Error($"Error consuming MTP message from MulticastId {_multicastId}.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"XML deserialization of MTP messaged failed for MulticastId {_multicastId} - ", ex);
                }
            }
            else
            {
                Logger.Error($"MTP datagram decryption failed for MulticastId {_multicastId}.");

                if (_consecutiveHashFailures >= 5 || _consecutiveMessageIdFailures >= 5)
                {
                    if (!_coordinationStopwatch.IsRunning || (_coordinationStopwatch.IsRunning && _coordinationStopwatch.ElapsedMilliseconds > MIN_MS_BETWEEN_COORDINATIONS))
                    {
                        ServiceManager.GetInstance().GetService<IEventBus>().Publish(new MtpKeyCoordinationNeededEvent {HostId = _communicationId, MulticastId = _multicastId });
                        _coordinationStopwatch = Stopwatch.StartNew();
                    }
                }
            }
        }
    }
}
