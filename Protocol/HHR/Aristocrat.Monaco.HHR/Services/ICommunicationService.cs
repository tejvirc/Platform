namespace Aristocrat.Monaco.Hhr.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Service facilitates connection, disconnection and heartbeats with Central Server.
    /// </summary>
    public interface ICommunicationService
    {
        /// <summary>
        ///     Connects TCP connection to the HHR server, so we can begin communication
        /// </summary>
        /// <param name="token">A token to cancel this operation</param>
        /// <returns>true if connection is successful, false otherwise</returns>
        Task<bool> ConnectTcp(CancellationToken token = default);

        /// <summary>
        ///     Connects UDP connection to the HHR server, once we receive UdpIp via TCP
        /// </summary>
        /// <param name="token">A token to cancel this operation</param>
        /// <returns>true if reconnection is successful, false otherwise</returns>
        Task<bool> ConnectUdp(CancellationToken token = default);

        /// <summary>
        ///     Disconnects all connections to the HHR server, for when we shut down
        /// </summary>
        /// <param name="token">A token to cancel this operation</param>
        /// <returns>true if disconnection is successful, false otherwise</returns>
        Task<bool> Disconnect(CancellationToken token = default);
    }
}