namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     The progressive message handler interface
    /// </summary>
    /// <typeparam name="TResponse">The response type for this message handler</typeparam>
    /// <typeparam name="TMessage">The message type to handle</typeparam>
    public interface IProgressiveMessageHandler<TResponse, in TMessage>
        where TResponse : IResponse
        where TMessage : IMessage
    {
        /// <summary>
        ///     Handles the progressive message from the bingo sever
        /// </summary>
        /// <typeparam name="TResponse">The response type to use for the message to handle</typeparam>
        /// <typeparam name="TMessage">The message type to handle</typeparam>
        /// <param name="message">The message to handle</param>
        /// <param name="token">The cancellation token to use</param>
        /// <returns>The task for handling the progressive message</returns>
        Task<TResponse> Handle(TMessage message, CancellationToken token = default);
    }
}