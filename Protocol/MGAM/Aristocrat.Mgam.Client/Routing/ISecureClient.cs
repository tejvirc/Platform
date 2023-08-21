namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Net;
    using Monaco.Protocol.Common.Communication;

    /// <summary>
    ///     Wrapper class around <see cref="SslClient"/>.
    /// </summary>
    internal interface ISecureClient : IDisposable
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
        ///     Gets or sets the IP endpoint.
        /// </summary>
        IPEndPoint Endpoint { get; }

        /// <summary>
        ///     Gets a value that indicates whether the client is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     Gets or sets the keep alive option.
        /// </summary>
        bool OptionKeepAlive { get; set; }

        /// <summary>
        ///     Gets or sets no delay option.
        /// </summary>
        bool OptionNoDelay { get; set; }

        /// <summary>
        ///     Initiates connection to the end point.
        /// </summary>
        /// <returns>'true' if the client was successfully connected, 'false' if the client failed to connect.</returns>
        bool ConnectAsync();

        /// <summary>
        ///     Initiates disconnection to the end point.
        /// </summary>
        /// <returns>'true' if the client was successfully disconnected, 'false' if the client failed to disconnect.</returns>
        bool DisconnectAsync();

        /// <summary>
        ///     Send data to the server.
        /// </summary>
        /// <param name="buffer">Buffer to send.</param>
        /// <returns>Size of sent data.</returns>
        bool SendAsync(byte[] buffer);
    }
}
