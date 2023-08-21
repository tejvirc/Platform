namespace Aristocrat.Mgam.Client.Routing
{
    using Messaging;

    /// <summary>
    ///     Reports transport status events.
    /// </summary>
    internal interface ITransportPublisher : ITransportStatusSubscription, IRoutedMessagesSubscription
    {
        /// <summary>
        ///     Add transport status to be published.
        /// </summary>
        /// <param name="status">New transport status event.</param>
        void Publish(TransportStatus status);

        /// <summary>
        ///     Add routed message to be published.
        /// </summary>
        /// <param name="source">The message origination point.</param>
        /// <param name="destination">The message destination point.</param>
        /// <param name="message">The message that is being routed.</param>
        /// <param name="xml">The message raw xml data.</param>
        void Publish(string source, string destination, IMessage message, string xml);
    }
}
