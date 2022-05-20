namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Message result.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public class MessageResult<TResponse>
        where TResponse : class, IResponse
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageResult{TResponse}"/> class.
        /// </summary>
        /// <param name="status">Message status.</param>
        /// <param name="response">Message response.</param>
        private MessageResult(MessageStatus status, TResponse response = null)
        {
            Status = status;
            Response = response;
        }

        /// <summary>
        ///     Factory method for creating a message result.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static MessageResult<TResponse> Create(MessageStatus status, TResponse response = null)
        {
            return new MessageResult<TResponse>(status, response);
        }

        /// <summary>
        ///     Get the status of the message.
        /// </summary>
        public MessageStatus Status { get; }

        /// <summary>
        ///     Gets the message response.
        /// </summary>
        public TResponse Response { get; }
    }
}
