namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Sends and receives messages to/from the host.
    /// </summary>
    internal interface ISecureTransporter
    {
        /// <summary>
        ///     Gets the endpoint of the VLT service.
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
        Task Send(Payload payload, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Connects to VLT Service end point.
        /// </summary>
        /// <param name="endPoint">VLT Service end point address.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        Task Connect(IPEndPoint endPoint, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Disconnects from VLT Service end point.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        Task Disconnect(CancellationToken cancellationToken = default);
    }
}
