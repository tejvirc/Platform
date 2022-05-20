namespace Aristocrat.Mgam.Client.Routing
{
    using System;

    /// <summary>
    ///     A message that was sent to or from the server.
    /// </summary>
    public struct RoutedMessage
    {
        /// <summary>
        ///     Gets or sets the time the message was sent.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     Gets or sets the source address.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Gets or sets the destination address.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        ///     Gets or sets the type of the message.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the message response code.
        /// </summary>
        public int? ResponseCode { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether the message is a request.
        /// </summary>
        public bool IsRequest { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether the message is a response.
        /// </summary>
        public bool IsResponse { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether the message is a command.
        /// </summary>
        public bool IsCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether the message is a notification.
        /// </summary>
        public bool IsNotification { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether the message is the keep-alive message.
        /// </summary>
        public bool IsHeartbeat { get; set; }

        /// <summary>
        ///     Gets or sets the message raw xml.
        /// </summary>
        public string RawData { get; set; }
    }
}
