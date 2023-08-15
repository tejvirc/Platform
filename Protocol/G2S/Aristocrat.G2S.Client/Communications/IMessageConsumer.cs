namespace Aristocrat.G2S.Client.Communications
{
    /// <summary>
    ///     Provides a mechanism to consume an inbound message. A message consumer will receive IPoint2Point messages when the
    ///     message is received on the communication channel exposed by the EGM.
    /// </summary>
    public interface IMessageConsumer
        : IMessageReceiverConnector
    {
        /// <summary>
        ///     Notifies the message consumer when a message is received on the communication channel exposed by the EGM.
        /// </summary>
        /// <param name="point2Point">The unprocessed message received on the channel.</param>
        /// <returns>Return an Error if message could not be processed or null upon success.</returns>
        Error Consumes(IPoint2Point point2Point);

        /// <summary>
        ///     Notifies the message consumer when a message is received on the communication channel exposed by the EGM.
        /// </summary>
        /// <param name="broadcast">The unprocessed message received on the channel.</param>
        /// <param name="hostId">The id of the host who sent the broadcast message.</param>
        /// <returns>Return an Error if message could not be processed or null upon success.</returns>
        Error Consumes(IBroadcast broadcast, int hostId);
    }
}