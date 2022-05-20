namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Net;
    using Monaco.Protocol.Common.Communication;

    /// <summary>
    ///     TCP SSL client.
    /// </summary>
    internal class SecureClient : SslClient, ISecureClient
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SecureClient"/> class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="endPoint"></param>
        public SecureClient(SslContext context, IPEndPoint endPoint)
            : base(context, endPoint)
        {
        }

        /// <summary>
        ///     This event is fired when the client is connected.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> Connected;

        /// <summary>
        ///     This event is fired when the client is disconnected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     This event is fired when the client is receives data.
        /// </summary>
        public event EventHandler<ReceivedEventArgs> Received;

        /// <summary>
        ///     This event is fired when the client encounters an error.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <inheritdoc />
        protected override void OnHandshaked()
        {
            RaiseConnected(Endpoint);
        }

        /// <inheritdoc />
        protected override void OnDisconnected()
        {
            RaiseDisconnected(Endpoint);
        }

        /// <inheritdoc />
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            RaiseReceived(Endpoint, buffer, offset, size);
        }

        protected override void OnError(Exception exception)
        {
            RaiseError(Endpoint, exception);
        }

        private void RaiseConnected(IPEndPoint endpoint)
        {
            Connected?.Invoke(this, new ConnectedEventArgs(endpoint));
        }

        private void RaiseDisconnected(IPEndPoint endpoint)
        {
            Disconnected?.Invoke(this, new DisconnectedEventArgs(endpoint));
        }

        private void RaiseReceived(IPEndPoint endpoint, byte[] buffer, long offset, long size)
        {
            Received?.Invoke(this, new ReceivedEventArgs(endpoint, buffer, offset, size));
        }

        private void RaiseError(IPEndPoint endpoint, Exception exception)
        {
            Error?.Invoke(this, new ErrorEventArgs(endpoint, exception));
        }
    }
}
