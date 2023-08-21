namespace Aristocrat.G2S.Client.Communications
{
    /// <summary>
    ///     Provides a mechanism for connecting a message consumer to a receive endpoint.
    /// </summary>
    /// <remarks>
    ///     This interface is provided as abstraction to the underlying G2S communication classes.  This was done to decouple
    ///     the underlying communication classes and provide the ability to write unit tests.
    /// </remarks>
    public interface IReceiveEndpointProvider : ICommunicator
    {
        /// <summary>
        ///     Connects a message consumer.
        /// </summary>
        /// <param name="consumer">Message Consumer instance that is supposed to be connected.</param>
        void ConnectConsumer(IMessageConsumer consumer);
    }
}