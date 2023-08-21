namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Net;
    using Monaco.Protocol.Common.Communication;

    /// <summary>
    ///     Wrapper class around <see cref="UdpClient"/>.
    /// </summary>
    internal interface IBroadcastClient : IDisposable
    {
        /// <summary>
        ///     This event is fired when the client is connected.
        /// </summary>
        event EventHandler<ConnectedEventArgs> Connected;

        /// <summary>
        ///     This event is fired when the client is disconnected.
        /// </summary>
        event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     This event is fired when the client is receives data.
        /// </summary>
        event EventHandler<ReceivedEventArgs> Received;

        /// <summary>
        ///     This event is fired when the client encounters an error.
        /// </summary>
        event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        ///     Gets or sets a value that indicates whether broadcasting is enabled.
        /// </summary>
        bool EnableBroadcast { get; set; }

        /// <summary>
        ///     Gets or sets the IP endpoint.
        /// </summary>
        IPEndPoint Endpoint { get; }

        /// <summary>
        ///     Gets a value that indicates whether the client is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     Gets or sets a value that indicates whether to bind the socket to the multicast UDP server.
        /// </summary>
        bool IsMulticast { get; set; }

        /// <summary>
        ///     Initiates connection to the end point.
        /// </summary>
        /// <returns>'true' if the client was successfully connected, 'false' if the client failed to connect.</returns>
        bool Connect();

        /// <summary>
        ///     Initiates disconnection to the end point.
        /// </summary>
        /// <returns>'true' if the client was successfully disconnected, 'false' if the client failed to disconnect.</returns>
        bool Disconnect();

        /// <summary>
        ///     Send datagram to the connected end point (synchronous).
        /// </summary>
        /// <returns>Size of sent datagram.</returns>
        long Send(EndPoint endPoint, byte[] buffer);
    }
}
