namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using System.Threading;
    using System.Threading.Tasks;
    using Communication;
    using Messages;
    using Monaco.Protocol.Common;

    /// <summary>
    ///     Provides message encoding and decoding using a flow of operations such as encryption and CRC, by applying them
    ///     in forward and reverse order depending on the required operation.
    /// </summary>
    public interface IMessageFlow
    {
        /// <summary>
        ///     Starts pipeline which will receive message and converts to HHRMessages, encrypt, adds crc and sends byte[]
        ///     to central server.
        /// </summary>
        /// <param name="message">Message which needs to send to central server.</param>
        /// <param name="token">Cancellation token to abort MessageFlow</param>
        Task<bool> Send(Request message, CancellationToken token = default);

        /// <summary>
        ///     Starts pipeline to prepare Aristocrat message from Packet received from Server after Decrypting, CRC validation and
        ///     Deserializing
        /// </summary>
        /// <param name="data">Packet from Central Server</param>
        /// <param name="token">A cancellation token to abort MessageFlow</param>
        /// <returns>A Response object containing the data that was on the received Packet</returns>
        Task<Response> Receive(Packet data, CancellationToken token = default);
    }
}