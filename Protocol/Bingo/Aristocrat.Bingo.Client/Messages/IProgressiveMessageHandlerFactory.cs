namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A factory for handling progressive messages from the bingo server
    /// </summary>
    public interface IProgressiveMessageHandlerFactory
    {
        /// <summary>
        ///     Handles the progressive message from the bingo server
        /// </summary>
        /// <typeparam name="TResponse">The response type to use for the message to handle</typeparam>
        /// <typeparam name="TMessage">The message type to handle</typeparam>
        /// <param name="message">The message to handle</param>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>The task for handling the message</returns>
        Task<TResponse> Handle<TResponse, TMessage>(TMessage message, CancellationToken token = default)
            where TResponse : IResponse
            where TMessage : IMessage;
    }
}
