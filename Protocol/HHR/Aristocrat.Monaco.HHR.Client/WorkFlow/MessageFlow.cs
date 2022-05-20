// ReSharper disable RedundantTypeSpecificationInDefaultExpression
namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Communication;
    using Data;
    using log4net;
    using Messages;

    /// <inheritdoc />
    public class MessageFlow : IMessageFlow
    {
        private readonly IMessageFactory _messageFactory;
        private readonly ICrcProvider _crcProvider;
        private readonly ICryptoProvider _cryptoProvider;
        private readonly ITcpConnection _tcpConnection;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Pipeline<Request, bool> _senderPipeline;
        private Pipeline<byte[], Response> _receiverPipeline;

        //private TaskCompletionSource<Response> _receiverPipelineOutcome;
        /// <summary>
        ///     Message Flow initialization
        /// </summary>
        /// <param name="messageFactory">Message Factory to Serialize and Deserialize</param>
        /// <param name="crcProvider">CrcProvider to calculate and append Crc</param>
        /// <param name="tcpConnection">TransportManager to send packet to Central Server</param>
        /// <param name="cryptoProvider">CryptoProvider to Encrypt/Decrypt messages</param>
        public MessageFlow(
            IMessageFactory messageFactory,
            ICrcProvider crcProvider,
            ITcpConnection tcpConnection,
            ICryptoProvider cryptoProvider)
        {
            _messageFactory = messageFactory;
            _crcProvider = crcProvider;
            _tcpConnection = tcpConnection;
            _cryptoProvider = cryptoProvider;

            InitializeSenderPipeline();
            InitializeReceiverPipeline();
        }

        /// <inheritdoc />
        public async Task<bool> Send(Request message, CancellationToken token = default)
        {
            return await _senderPipeline.Execute(message, token);
        }

        /// <inheritdoc />
        public async Task<Response> Receive(Packet data, CancellationToken token = default)
        {
            return await _receiverPipeline.Execute(data.Data, token);
        }

        private void InitializeSenderPipeline()
        {
            _senderPipeline = new Pipeline<Request, bool>().AddBlock<Request, (byte[], Request)>(
                    request => (_messageFactory.Serialize(request), request))
                .AddBlock<(byte[], Request), byte[]>(
                    tuple =>
                    {
                        var (bytes, request) = tuple;

                        return MessageUtility.PopulateMessageHeader(request, bytes);
                    },
                    error => { _logger.Error("[SENDPIPE] Unable to populate MessageHeader.", error); })
                .AddBlock<byte[], (byte[], ushort)>(
                    data => Task.Run(
                            async () => (await _cryptoProvider.Encrypt(data), _crcProvider.Calculate(data))),
                    error => { _logger.Error("[SENDPIPE] Failed to Encrypt message data.", error); })
                .AddBlock<(byte[], ushort), byte[]>(
                    tuple =>
                    {
                        var (bytes, crc) = tuple;
                        var header = new MessageEncryptHeader
                        {
                            EncryptionType = _cryptoProvider.GetEncryptionType(),
                            EncryptionLength = bytes.Length + Marshal.SizeOf<MessageEncryptHeader>(),
                            Crc = crc
                        };

                        _logger.Debug($"[SENDPIPE] Add EncryptHeader - [Len={header.EncryptionLength},  Crc={header.Crc}]");

                        var withHeader = MessageUtility.WrapBytesWithMessage(bytes, header);

                        return withHeader;
                    },
                    error => { _logger.Error("[SENDPIPE] Unable to append Encrypted header into data bytes", error); })
                .AddBlock<byte[], bool>(
                    bytes => _tcpConnection.SendBytes(bytes),
                    error => { _logger.Error("[SENDPIPE] Unable to Send data over TcpConnection.", error); })
                .CreatePipeline();
        }

        private void InitializeReceiverPipeline()
        {
            _receiverPipeline = new Pipeline<byte[], Response>()
                .AddBlock<byte[], (byte[], byte[])>(
                    bytes => Task.Run(async () => (bytes, await _cryptoProvider.Decrypt(MessageUtility.ExtractEncryptedHeader(bytes)))),
                    error => { _logger.Error("[RECVPIPE] Unable to Decrypt data.", error); })
                .AddBlock<(byte[], byte[]), byte[]>(tuple =>
                {
                    var (encryptedBytes, decryptedHeader) = tuple;
                    var calculatedCrc = _crcProvider.Calculate(decryptedHeader);

                    if (calculatedCrc == MessageUtility.GetMessage<MessageEncryptHeader>(encryptedBytes).Crc)
                    {
                        return decryptedHeader;
                    }

                    _logger.Error($"[RECVPIPE] CRC Mismatch - [Expected={MessageUtility.GetMessage<MessageEncryptHeader>(encryptedBytes).Crc}, CalculatedCrc={calculatedCrc}]");

                    throw new InvalidDataException("CRC of incoming packet doesn't match");

                })
                .AddBlock<byte[], Response>(
                    data =>
                    {
                        var resp= _messageFactory.Deserialize(data);

                        return resp;
                    },
                    error => { _logger.Error("[RECVPIPE] Unable to DeSerialize data.", error); })
                .CreatePipeline();
        }
    }
}