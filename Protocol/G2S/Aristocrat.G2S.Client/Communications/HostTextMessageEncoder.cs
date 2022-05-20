namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.IO;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;

    /// <summary>
    ///     Custom text message encoder used to handle null/empty content types
    /// </summary>
    public class HostTextMessageEncoder : MessageEncoder
    {
        private readonly HostTextMessageEncoderFactory _factory;
        private readonly XmlWriterSettings _writerSettings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTextMessageEncoder" /> class.
        /// </summary>
        /// <param name="factory">A <see cref="HostTextMessageEncoder" /> instance</param>
        public HostTextMessageEncoder(HostTextMessageEncoderFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            _writerSettings = new XmlWriterSettings { Encoding = Encoding.GetEncoding(factory.CharSet) };
            ContentType = $"{_factory.MediaType}; charset={_writerSettings.Encoding.HeaderName}";
        }

        /// <inheritdoc />
        public override string ContentType { get; }

        /// <inheritdoc />
        public override string MediaType => _factory.MediaType;

        /// <inheritdoc />
        public override MessageVersion MessageVersion => _factory.MessageVersion;

        /// <inheritdoc />
        public override bool IsContentTypeSupported(string contentType)
        {
            // It appears the transport is setting the content type to "application/octet-stream" when/if it's empty.
            //   This typically wouldn't be advisable, but we had to deal with the IGT host not sending the content type in the header.
            if (base.IsContentTypeSupported(contentType) ||
                string.IsNullOrEmpty(contentType) ||
                string.Equals(contentType, "application/octet-stream", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (MediaType == null || contentType.Length == MediaType.Length)
            {
                return contentType.Equals(MediaType, StringComparison.OrdinalIgnoreCase);
            }

            return contentType.StartsWith(MediaType, StringComparison.OrdinalIgnoreCase) &&
                   contentType[MediaType.Length] == ';';
        }

        /// <inheritdoc />
        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            if (buffer.Array == null)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(buffer));
            }

            var msgContents = new byte[buffer.Count];
            Array.Copy(buffer.Array, buffer.Offset, msgContents, 0, msgContents.Length);
            bufferManager.ReturnBuffer(buffer.Array);

            var stream = new MemoryStream(msgContents);
            return ReadMessage(stream, int.MaxValue);
        }

        /// <inheritdoc />
        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            var reader = XmlReader.Create(stream);
            return Message.CreateMessage(reader, maxSizeOfHeaders, MessageVersion);
        }

        /// <inheritdoc />
        public override ArraySegment<byte> WriteMessage(
            Message message,
            int maxMessageSize,
            BufferManager bufferManager,
            int messageOffset)
        {
            var stream = new MemoryStream();
            var writer = XmlWriter.Create(stream, _writerSettings);
            message.WriteMessage(writer);
            writer.Close();

            var messageBytes = stream.GetBuffer();
            var messageLength = (int)stream.Position;

            var totalLength = messageLength + messageOffset;
            var totalBytes = bufferManager.TakeBuffer(totalLength);
            Array.Copy(messageBytes, 0, totalBytes, messageOffset, messageLength);

            return new ArraySegment<byte>(totalBytes, messageOffset, messageLength);
        }

        /// <inheritdoc />
        public override void WriteMessage(Message message, Stream stream)
        {
            var writer = XmlWriter.Create(stream, _writerSettings);
            message.WriteMessage(writer);
            writer.Close();
        }
    }
}