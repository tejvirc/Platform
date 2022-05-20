namespace Aristocrat.Mgam.Client
{
    /// <summary>
    ///     Defines constants for the Mgam protocol.
    /// </summary>
    public static class ProtocolConstants
    {
        /// <summary>Manufacturer 3-letter code.</summary>
        public const string ManufacturerPrefix = @"ATI";

        /// <summary>Compressed message header.</summary>
        public const string XmlCompressedDataSet = "XMLCompressedDataSet";

        /// <summary>Non compressed message header.</summary>
        public const string XmlDataSet = "XMLDataSet";

        /// <summary>Default compression threshold.</summary>
        public const int DefaultCompressionThreshold = 400;

        /// <summary>Default connection timeout.</summary>
        public const int DefaultConnectionTimeout = 6;

        /// <summary>Default keep alive interval.</summary>
        public const int DefaultKeepAliveInterval = 3;

        /// <summary>Default number of message request retry attempts.</summary>
        public const int DefaultRetries = 3;

        /// <summary>Default number of connection retry attempts.</summary>
        public const int DefaultConnectionRetries = 5;

        /// <summary>Default delay between retries (in seconds).</summary>
        public const int DefaultRetryDelay = 3;

        /// <summary>Default notification queue size.</summary>
        public const int DefaultNotificationQueueSize = 20;
    }
}
