namespace Aristocrat.G2S.Client.Communications
{
    /// <summary>
    ///     Provides a mechanism to connect a message receive.
    /// </summary>
    public interface IMessageReceiverConnector
    {
        /// <summary>
        ///     Connects the instance of the <see cref="IMessageReceiver" />
        /// </summary>
        /// <param name="observer"><see cref="IMessageReceiver" /> instance that should be connected.</param>
        void Connect(IMessageReceiver observer);

        /// <summary>
        ///     Disconnects the instance of the <see cref="IMessageReceiver" />
        /// </summary>
        /// <param name="observer"><see cref="IMessageReceiver" /> instance that should be disconnected.</param>
        void Disconnect(IMessageReceiver observer);
    }
}