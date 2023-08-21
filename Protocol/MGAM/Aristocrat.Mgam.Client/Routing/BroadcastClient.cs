namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using UdpClient = Monaco.Protocol.Common.Communication.UdpClient;

    /// <summary>
    ///     Wrapper class around <see cref="UdpClient"/> for raising event notifications.
    /// </summary>
    internal class BroadcastClient : UdpClient, IBroadcastClient
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BroadcastClient"/> class.
        /// </summary>
        /// <param name="endPoint">Service address.</param>
        public BroadcastClient(IPEndPoint endPoint)
            : base(endPoint)
        {
        }

        public bool EnableBroadcast
        {
            get => Socket.EnableBroadcast;

            set => Socket.EnableBroadcast = value;
        }

        /// <inheritdoc />
        public event EventHandler<ConnectedEventArgs> Connected;

        /// <inheritdoc />
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <inheritdoc />
        public event EventHandler<ReceivedEventArgs> Received;

        /// <inheritdoc />
        public event EventHandler<ErrorEventArgs> Error;

        /// <inheritdoc />
        protected override void OnConnected()
        {
            RaiseConnected(Endpoint);

            ReceiveAsync();
        }

        /// <inheritdoc />
        protected override void OnDisconnected()
        {
            RaiseDisconnected(Endpoint);
        }

        /// <inheritdoc />
        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            RaiseReceived(endpoint, buffer, offset, size);

            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
        {
            RaiseError(Endpoint, new SocketException((int)error));
        }

        private void RaiseConnected(IPEndPoint endpoint)
        {
            Connected?.Invoke(this, new ConnectedEventArgs(endpoint));
        }

        private void RaiseDisconnected(IPEndPoint endpoint)
        {
            Disconnected?.Invoke(this, new DisconnectedEventArgs(endpoint));
        }

        private void RaiseReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            Received?.Invoke(this, new ReceivedEventArgs((IPEndPoint)endpoint, buffer, offset, size));
        }

        private void RaiseError(IPEndPoint endpoint, Exception exception)
        {
            Error?.Invoke(this, new ErrorEventArgs(endpoint, exception));
        }
    }
}
