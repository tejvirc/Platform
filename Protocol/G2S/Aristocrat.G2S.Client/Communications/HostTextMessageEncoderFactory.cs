namespace Aristocrat.G2S.Client.Communications
{
    using System.ServiceModel.Channels;

    /// <summary>
    ///     A custom text message encoder factory used to support a null/content type
    /// </summary>
    public class HostTextMessageEncoderFactory : MessageEncoderFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTextMessageEncoderFactory" /> class.
        /// </summary>
        /// <param name="mediaType">The media type</param>
        /// <param name="charSet">The character set</param>
        /// <param name="version">The message version</param>
        internal HostTextMessageEncoderFactory(string mediaType, string charSet, MessageVersion version)
        {
            MessageVersion = version;
            MediaType = mediaType;
            CharSet = charSet;
            Encoder = new HostTextMessageEncoder(this);
        }

        /// <inheritdoc />
        public override MessageEncoder Encoder { get; }

        /// <inheritdoc />
        public override MessageVersion MessageVersion { get; }

        /// <summary>
        ///     Gets the media type
        /// </summary>
        internal string MediaType { get; }

        /// <summary>
        ///     Gets the character set
        /// </summary>
        internal string CharSet { get; }
    }
}