namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Net;
    using Routing;

    /// <summary>
    ///     Message payload.
    /// </summary>
    internal struct Payload
    {
        /// <summary>
        ///     Gets or sets the host address for the message.
        /// </summary>
        public IPEndPoint EndPoint { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether this is a broadcast message.
        /// </summary>
        public bool IsBroadcast { get; set; }

        /// <summary>
        ///     Gets or sets the format of the payload or content of the message.
        /// </summary>
        public XmlDataSetFormat Format { get; set; }

        /// <summary>
        ///     Gets or sets the size of the message payload in bytes (not including the header).
        /// </summary>
        public int MessageSize { get; set; }

        /// <summary>
        ///     Gets or sets the message content.
        /// </summary>
        public byte[] Content { get; set; }
    }
}
