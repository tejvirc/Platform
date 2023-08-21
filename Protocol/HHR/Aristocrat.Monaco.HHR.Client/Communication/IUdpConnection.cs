namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Interface for UDP part of communication between gaming terminal and central server
    /// </summary>
    public interface IUdpConnection : IConnectionReader
    {
        /// <summary>
        ///     Start listening to the specified multicast group
        /// </summary>
        /// <param name="endPoint">Multicast group we need to listen to.</param>
        /// <param name="token">A cancellation token to make this operation cancellable.</param>
        /// <returns>True if the connection was successful</returns>
        Task<bool> Open(IPEndPoint endPoint, CancellationToken token = default);

        /// <summary>
        ///     Close the connection to the multicast group
        /// </summary>
        /// <param name="token">A cancellation token to make this operation cancellable.</param>
        /// <returns>A task that can be monitored if we wish to wait until complete</returns>
        Task Close(CancellationToken token = default);
    }
}