namespace Aristocrat.Mgam.Client.Routing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Sends messages to the VLT service.
    /// </summary>
    public interface IRequestRouter
    {
        /// <summary>
        ///     Queue a message to be sent to the VLT service.
        /// </summary>
        /// <param name="request">Message to send.</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="Task"/> message response.</returns>
        Task<MessageResult<TResponse>> Send<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : class, IRequest
            where TResponse : class, IResponse;
    }
}
