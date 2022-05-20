namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Common interface for managing network connections which can be connected
    /// </summary>
    public interface IConnectionOpener
    {
        /// <summary>
        ///     A ConnectionStatus which indicates whether the connection is currently opened or closed
        /// </summary>
        ConnectionStatus CurrentStatus { get; }

        /// <summary>
        ///     An observable ConnectionStatus which will notify all its subscribers about connection status changes
        /// </summary>
        IObservable<ConnectionStatus> ConnectionStatus { get; }

        /// <summary>
        ///     Open a connection to the server at the specified endpoint
        /// </summary>
        /// <param name="endPoint">Endpoint where we need to connect to.</param>
        /// <param name="token">A cancellation token to make this operation cancellable.</param>
        /// <returns>True if the connection was successful</returns>
        Task<bool> Open(IPEndPoint endPoint, CancellationToken token = default);

        /// <summary>
        ///     Close the connection to the remote server
        /// </summary>
        /// <param name="token">A cancellation token to make this operation cancellable.</param>
        /// <returns>A task that can be monitored if we wish to wait until complete</returns>
        Task Close(CancellationToken token = default);
    }
}