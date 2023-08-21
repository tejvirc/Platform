namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Interface for TCP part of communication between gaming terminal and central server
    /// </summary>
    public interface ITcpConnection : IConnectionOpener, IConnectionReader
    {
        /// <summary>
        ///     Sends data to the remote server
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task<bool> SendBytes(byte[] data, CancellationToken token = default);
    }
}