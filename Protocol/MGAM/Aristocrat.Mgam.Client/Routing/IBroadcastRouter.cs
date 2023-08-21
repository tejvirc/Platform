namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Broadcast message request to the Directory service.
    /// </summary>
    internal interface IBroadcastRouter
    {
        /// <summary>
        ///     Queue a message to be broadcast to the Directory service.
        /// </summary>
        /// <param name="request">Message to broadcast.</param>
        /// <param name="subscriber">Response listener.</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="Task"/>.</returns>
        Task<IDisposable> Broadcast<TRequest, TResponse>(
            TRequest request,
            Action<TResponse> subscriber,
            CancellationToken cancellationToken = default)
            where TRequest : class, IRequest
            where TResponse : class, IResponse;
    }
}
