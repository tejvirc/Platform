namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Broadcasts messages to Directory Service.
    /// </summary>
    internal interface IBroadcastTransporter
    {
        /// <summary>
        ///     Gets the endpoint of the Directory service.
        /// </summary>
        IPEndPoint EndPoint { get; }

        /// <summary>
        ///     Gets an observable that notifies when a message is received.
        /// </summary>
        IObservable<Payload?> Messages { get; }

        /// <summary>
        ///     Broadcast a message to the network to be received by the site-controller.
        /// </summary>
        /// <param name="payload">Message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task Broadcast(Payload payload, CancellationToken cancellationToken = default);
    }
}
