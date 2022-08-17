namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Net;
    using System.Reflection;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Protocol.Common;
    using Devices.v21;
    using log4net;
    using Monaco.Protocol.Common.Communication;

    /// <summary>
    ///     Client for receiving MTP messages from the G2S host.
    /// </summary>
    public class MtpClient : IMtpClient
    {
        private readonly MessageBuilder _messageBuilder = new MessageBuilder();
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private UdpConnection _connection;
        private IMessageConsumer _consumer;
        private IPEndPoint _multicastAddress;
        private bool _disposed;
        private int _consecutiveHashFailures;
        private int _consecutiveMessageIdFailures;

        /// <inheritdoc />
        public IPEndPoint MulticastAddress
        {
            get { return _multicastAddress; }
        }

        // TODO Update these coordinator values via the handler for joinMcast and also mcastKeyUpdate - They can both contain the security params.
        /// <inheritdoc />
        public string MulticastId { get; set; }

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
        public Uri Address
        {
            get
            {
                return new Uri(_multicastAddress.Address.ToString());
            }
        }

        /// <inheritdoc />
        public void Open(ICommunicationsDevice device)
        {
            if (device == null || device.AllowMulticast)
            {
                Logger.Error("No multicast capable communication device found to open the MTP connection.");
                return;
            }

            SetMtpSecurityParameters(device.Key, device.MessageId, device.NextKey, device.KeyChangeId);
            _multicastAddress = device.MulticastAddress;
            if (_multicastAddress == null)
            {
                Logger.Error("No multicast address is set for MTP.");
                return;
            }

            if (_connection != null)
                _connection.Dispose();

            _connection = new UdpConnection();
            var success = _connection.Open(MulticastAddress);
            if (success.Result)
            {
                _connection.IncomingBytes.Subscribe(OnProgressiveUpdateReceived);
                _disposed = false;
            }
            else
            {
                Logger.Error("Unable to connect to MTP server");
            }
        }

        /// <inheritdoc />
        public void Close()
        {
            _connection?.Close();
        }

        /// <inheritdoc />
        private void OnProgressiveUpdateReceived(Packet packet)
        {
            var decryptedMessage = "";

            try
            {
                decryptedMessage = _messageBuilder.DecodeDatagram(packet.Data, this);
            }
            catch (MtpHashException ex)
            {
                Logger.Error("MTP hash failed validation. ", ex);
                _consecutiveHashFailures++;
            }
            catch (MtpMessageIdException ex)
            {
                Logger.Error("Out of order MTP message ID received. ", ex);
                _consecutiveMessageIdFailures++;
            }
            catch (Exception ex)
            {
                Logger.Error("Invalid MTP packet received. ", ex);
            }

            if (decryptedMessage.Length > 0) // The decryption above was successful
            {
                _consecutiveHashFailures = 0;
                _consecutiveMessageIdFailures = 0;

                try
                {
                    var deserializedMessage = _messageBuilder.DecodeMessage(decryptedMessage);
                    var multicast = deserializedMessage?.Item as IMulticast;
                    if (multicast == null)
                    {
                        Logger.Error("Invalid multicast message received over MTP.");
                        return;
                    }

                    if (_consumer == null)
                    {
                        Logger.Error("No MTP message consumers attached.");
                        return;
                    }

                    var error = _consumer.Consumes(multicast);

                    if (error.IsError)
                    {
                        Logger.Error("Error consuming MTP message.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("XML deserialization of MTP messaged failed. ", ex);
                }
            }
            else
            {
                Logger.Error("MTP datagram decryption failed.");
                if (_consecutiveHashFailures >= 5 || _consecutiveMessageIdFailures >= 5)
                {
                    ServiceManager.GetInstance().GetService<IEventBus>().Publish(new MtpKeyCoordinationNeededEvent());
                }
            }
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

            _connection?.Dispose();

            _disposed = true;
        }

        /// <inheritdoc />
        public void SetMtpSecurityParameters(byte[] key, long messageId, byte[] nextKey, long keyChangeId)
        {
            Key = key;
            LastMessageId = -1;
            MessageId = messageId;
            NextKey = nextKey;
            KeyChangeId = keyChangeId;
        }

        /// <inheritdoc />
        public void SetMulticastAddress(string address)
        {
            _multicastAddress = EndpointUtilities.CreateIPEndPoint(address);
        }
    }
}