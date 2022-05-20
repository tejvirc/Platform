namespace Aristocrat.Mgam.Client.Messaging
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Base implementation for handling messages and commands from the site controller.
    /// </summary>
    /// <typeparam name="TMessage"><see cref="IMessage"/> derived type.</typeparam>
    public abstract class MessageHandler<TMessage> : IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        /// <inheritdoc />
        public abstract Task<IResponse> Handle(TMessage message);

        /// <summary>
        ///     Sets the response success error code.
        /// </summary>
        /// <returns></returns>
        protected IResponse Ok<TResponse>(Action<TResponse> action = null)
            where TResponse : Response, new()
        {
            var response = new TResponse { ResponseCode = ServerResponseCode.Ok };

            action?.Invoke(response);

            return response;
        }

        /// <summary>
        ///     Sets the response success error code.
        /// </summary>
        /// <returns></returns>
        protected IResponse Response<TResponse>(ServerResponseCode code, Action<TResponse> action = null)
            where TResponse : Response, new()
        {
            var response = new TResponse { ResponseCode = code };

            action?.Invoke(response);

            return response;
        }
    }
}
