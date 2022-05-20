namespace Aristocrat.Monaco.Protocol.Common.Communication
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     UDP client is used to read/write data from/into the connected UDP server.
    /// </summary>
    public class UdpClient : IDisposable
    {
        private EndPoint _receiveEndpoint;
        private EndPoint _sendEndpoint;

        private bool _receiving;
        private Buffer _receiveBuffer;
        private SocketAsyncEventArgs _receiveEventArg;

        private bool _sending;
        private Buffer _sendBuffer;
        private SocketAsyncEventArgs _sendEventArg;
        private int _sendThreadId;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="address">IP address.</param>
        /// <param name="port">Port number.</param>
        public UdpClient(IPAddress address, int port)
            : this(new IPEndPoint(address, port))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="address">IP address.</param>
        /// <param name="port">Port number.</param>
        public UdpClient(string address, int port)
            : this(new IPEndPoint(IPAddress.Parse(address), port))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="endpoint">IP endpoint.</param>
        public UdpClient(IPEndPoint endpoint)
        {
            Id = Guid.NewGuid();
            Endpoint = endpoint;
        }

        /// <inheritdoc />
        ~UdpClient()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        /// <summary>
        ///     Gets or sets the client ID.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     Gets or sets the IP endpoint.
        /// </summary>
        public IPEndPoint Endpoint { get; }

        /// <summary>
        ///     Gets or sets the socket.
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        ///     Gets or sets the number of bytes pending sent by the client.
        /// </summary>
        public long BytesPending { get; private set; }

        /// <summary>
        ///     Gets or sets the number of bytes sending by the client.
        /// </summary>
        public long BytesSending { get; private set; }

        /// <summary>
        ///     Gets or sets the number of bytes sent by the client.
        /// </summary>
        public long BytesSent { get; private set; }

        /// <summary>
        ///     Gets or sets the number of bytes received by the client.
        /// </summary>
        public long BytesReceived { get; private set; }

        /// <summary>
        ///     Gets or sets the number of datagrams sent by the client.
        /// </summary>
        public long DatagramsSent { get; private set; }

        /// <summary>
        ///     Gets or sets the number of datagrams received by the client.
        /// </summary>
        public long DatagramsReceived { get; private set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether to reuse address.
        /// </summary>
        /// <remarks>
        ///     This option will enable/disable SO_REUSEADDR if the OS support this feature
        /// </remarks>
        public bool OptionReuseAddress { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether to enable a socket to be bound for exclusive access.
        /// </summary>
        /// <remarks>
        ///     This option will enable/disable SO_EXCLUSIVEADDRUSE if the OS support this feature
        /// </remarks>
        public bool OptionExclusiveAddressUse { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether to bind the socket to the multicast UDP server.
        /// </summary>
        public bool IsMulticast { get; set; }

        /// <summary>
        /// Option: receive buffer size
        /// </summary>
        public int OptionReceiveBufferSize { get; set; } = 8192;

        /// <summary>
        /// Option: send buffer size
        /// </summary>
        public int OptionSendBufferSize { get; set; } = 8192;

        /// <summary>
        ///     Gets or sets a value that indicates whether client is connected.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        ///     Client socket disposed flag.
        /// </summary>
        public bool IsSocketDisposed { get; private set; } = true;

        /// <summary>
        ///     Connect the client (synchronous).
        /// </summary>
        /// <returns>'true' if the client was successfully connected, 'false' if the client failed to connect.</returns>
        public virtual bool Connect()
        {
            if (IsConnected)
            {
                return false;
            }

            _receiveBuffer = new Buffer();
            _sendBuffer = new Buffer();

            _receiveEventArg = new SocketAsyncEventArgs();
            _receiveEventArg.Completed += OnAsyncCompleted;

            _sendEventArg = new SocketAsyncEventArgs();
            _sendEventArg.Completed += OnAsyncCompleted;

            Socket = new Socket(Endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.IpTimeToLive, OptionReuseAddress);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, OptionExclusiveAddressUse);

            Socket.Bind(IsMulticast ? Endpoint : new IPEndPoint(Endpoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0));

            _receiveEndpoint = new IPEndPoint(Endpoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);

            _receiveBuffer.Reserve(OptionReceiveBufferSize);

            BytesPending = 0L;
            BytesSending = 0L;
            BytesSent = 0L;
            BytesReceived = 0L;
            DatagramsSent = 0L;
            DatagramsReceived = 0L;
            IsConnected = true;

            OnConnected();

            return true;
        }

        /// <summary>
        ///     Disconnect the client (synchronous).
        /// </summary>
        /// <returns>'true' if the client was successfully disconnected, 'false' if the client is already disconnected</returns>
        public virtual bool Disconnect()
        {
            if (!IsConnected)
            {
                return false;
            }

            _receiveEventArg.Completed -= OnAsyncCompleted;
            _sendEventArg.Completed -= OnAsyncCompleted;

            try
            {
                Socket.Close();

                Socket.Dispose();

                IsSocketDisposed = true;
            }
            catch (ObjectDisposedException) { }

            IsConnected = false;

            _receiving = false;
            _sending = false;

            ClearBuffers();

            OnDisconnected();

            return true;
        }

        /// <summary>
        ///     Reconnect the client (synchronous).
        /// </summary>
        /// <returns>'true' if the client was successfully reconnected, 'false' if the client is already reconnected</returns>
        public virtual bool Reconnect()
        {
            return Disconnect() && Connect();
        }

        /// <summary>
        ///     Setup multicast: bind the socket to the multicast UDP server.
        /// </summary>
        /// <param name="enable">Enable/disable multicast</param>
        public virtual void SetupMulticast(bool enable)
        {
            OptionReuseAddress = enable;
            IsMulticast = enable;
        }

        /// <summary>
        ///     Join multicast group with a given IP address (synchronous).
        /// </summary>
        /// <param name="address">IP address</param>
        public virtual void JoinMulticastGroup(IPAddress address)
        {
            if (Endpoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(address));
            }
            else
            {
                Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(address));
            }

            // Call the client joined multicast group notification
            OnJoinedMulticastGroup(address);
        }
        /// <summary>
        ///     Join multicast group with a given IP address (synchronous).
        /// </summary>
        /// <param name="address">IP address.</param>
        public virtual void JoinMulticastGroup(string address) => JoinMulticastGroup(IPAddress.Parse(address));

        /// <summary>
        /// Leave multicast group with a given IP address (synchronous)
        /// </summary>
        /// <param name="address">IP address.</param>
        public virtual void LeaveMulticastGroup(IPAddress address)
        {
            if (Endpoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, new IPv6MulticastOption(address));
            }
            else
            {
                Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(address));
            }

            // Call the client left multicast group notification
            OnLeftMulticastGroup(address);
        }
        /// <summary>
        ///     Leave multicast group with a given IP address (synchronous).
        /// </summary>
        /// <param name="address">IP address.</param>
        public virtual void LeaveMulticastGroup(string address) => LeaveMulticastGroup(IPAddress.Parse(address));

        /// <summary>
        ///     Send datagram to the connected server (synchronous).
        /// </summary>
        /// <param name="buffer">Datagram buffer to send.</param>
        /// <returns>Size of sent datagram.</returns>
        public virtual long Send(byte[] buffer) => Send(buffer, 0, buffer.Length);

        /// <summary>
        ///     Send datagram to the connected server (synchronous).
        /// </summary>
        /// <param name="buffer">Datagram buffer to send.</param>
        /// <param name="offset">Datagram buffer offset.</param>
        /// <param name="size">Datagram buffer size.</param>
        /// <returns>Size of sent datagram.</returns>
        public virtual long Send(byte[] buffer, long offset, long size) => Send(Endpoint, buffer, offset, size);

        /// <summary>
        ///     Send text to the connected server (synchronous).
        /// </summary>
        /// <param name="text">Text string to send.</param>
        /// <returns>Size of sent datagram.</returns>
        public virtual long Send(string text) => Send(Encoding.UTF8.GetBytes(text));

        /// <summary>
        /// Send datagram to the given endpoint (synchronous).
        /// </summary>
        /// <param name="endpoint">Endpoint to send.</param>
        /// <param name="buffer">Datagram buffer to send.</param>
        /// <returns>Size of sent datagram.</returns>
        public virtual long Send(EndPoint endpoint, byte[] buffer) => Send(endpoint, buffer, 0, buffer.Length);

        /// <summary>
        /// Send datagram to the given endpoint (synchronous).
        /// </summary>
        /// <param name="endpoint">Endpoint to send.</param>
        /// <param name="buffer">Datagram buffer to send.</param>
        /// <param name="offset">Datagram buffer offset.</param>
        /// <param name="size">Datagram buffer size.</param>
        /// <returns>Size of sent datagram.</returns>
        public virtual long Send(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            if (!IsConnected)
            {
                return 0;
            }

            if (size == 0L)
            {
                return 0;
            }

            try
            {
                var num = Socket.SendTo(buffer, (int) offset, (int) size, SocketFlags.None, endpoint);
                if (num > 0)
                {
                    ++DatagramsSent;
                    BytesSent += num;
                    OnSent(endpoint, num);
                }

                return num;
            }
            catch (ObjectDisposedException)
            {
                return 0;
            }
            catch (SocketException ex)
            {
                SendError(ex.SocketErrorCode);
                Disconnect();
                return 0;
            }
        }

        /// <summary>
        ///     Send text to the given endpoint (synchronous).
        /// </summary>
        /// <param name="endpoint">Endpoint to send.</param>
        /// <param name="text">Text string to send.</param>
        /// <returns>Size of sent datagram.</returns>
        public virtual long Send(EndPoint endpoint, string text) => Send(endpoint, Encoding.UTF8.GetBytes(text));

        /// <summary>
        ///     Send datagram to the connected server (asynchronous).
        /// </summary>
        /// <param name="buffer">Datagram buffer to send.</param>
        /// <returns>'true' if the datagram was successfully sent, 'false' if the datagram was not sent.</returns>
        public virtual bool SendAsync(byte[] buffer) => SendAsync(buffer, 0, buffer.Length);

        /// <summary>
        ///     Send datagram to the connected server (asynchronous).
        /// </summary>
        /// <param name="buffer">Datagram buffer to send.</param>
        /// <param name="offset">Datagram buffer offset.</param>
        /// <param name="size">Datagram buffer size.</param>
        /// <returns>'true' if the datagram was successfully sent, 'false' if the datagram was not sent.</returns>
        public virtual bool SendAsync(byte[] buffer, long offset, long size) => SendAsync(Endpoint, buffer, offset, size);

        /// <summary>
        ///     Send text to the connected server (asynchronous).
        /// </summary>
        /// <param name="text">Text string to send.</param>
        /// <returns>'true' if the text was successfully sent, 'false' if the text was not sent.</returns>
        public virtual bool SendAsync(string text) => SendAsync(Encoding.UTF8.GetBytes(text));

        /// <summary>
        ///     Send datagram to the given endpoint (asynchronous).
        /// </summary>
        /// <param name="endpoint">Endpoint to send.</param>
        /// <param name="buffer">Datagram buffer to send.</param>
        /// <returns>'true' if the datagram was successfully sent, 'false' if the datagram was not sent.</returns>
        public virtual bool SendAsync(EndPoint endpoint, byte[] buffer) => SendAsync(endpoint, buffer, 0, buffer.Length);

        /// <summary>
        ///     Send datagram to the given endpoint (asynchronous).
        /// </summary>
        /// <param name="endpoint">Endpoint to send.</param>
        /// <param name="buffer">Datagram buffer to send.</param>
        /// <param name="offset">Datagram buffer offset.</param>
        /// <param name="size">Datagram buffer size.</param>
        /// <returns>'true' if the datagram was successfully sent, 'false' if the datagram was not sent.</returns>
        public virtual bool SendAsync(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            if (_sending)
            {
                return false;
            }

            if (!IsConnected)
            {
                return false;
            }

            if (size == 0)
            {
                return true;
            }

            _sendBuffer.Append(buffer, offset, size);

            BytesSending = _sendBuffer.Size;

            _sendEndpoint = endpoint;

            Task.Factory.StartNew(TrySend);

            return true;
        }

        /// <summary>
        ///     Send text to the given endpoint (asynchronous).
        /// </summary>
        /// <param name="endpoint">Endpoint to send</param>
        /// <param name="text">Text string to send</param>
        /// <returns>'true' if the text was successfully sent, 'false' if the text was not sent</returns>
        public virtual bool SendAsync(EndPoint endpoint, string text) => SendAsync(endpoint, Encoding.UTF8.GetBytes(text));

        /// <summary>
        ///     Receive a new datagram from the given endpoint (synchronous).
        /// </summary>
        /// <param name="endpoint">Endpoint to receive from</param>
        /// <param name="buffer">Datagram buffer to receive</param>
        /// <returns>Size of received datagram</returns>
        public virtual long Receive(ref EndPoint endpoint, byte[] buffer) => Receive(ref endpoint, buffer, 0, buffer.Length);

        /// <summary>
        ///     Receive a new datagram from the given endpoint (synchronous).
        /// </summary>
        /// <param name="endpoint">Endpoint to receive from</param>
        /// <param name="buffer">Datagram buffer to receive</param>
        /// <param name="offset">Datagram buffer offset</param>
        /// <param name="size">Datagram buffer size</param>
        /// <returns>Size of received datagram</returns>
        public virtual long Receive(ref EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            if (!IsConnected)
            {
                return 0;
            }

            if (size == 0)
            {
                return 0;
            }

            try
            {
                var received = Socket.ReceiveFrom(buffer, (int)offset, (int)size, SocketFlags.None, ref endpoint);
                if (received <= 0)
                {
                    return received;
                }

                DatagramsReceived++;
                BytesReceived += received;

                OnReceived(endpoint, buffer, offset, size);

                return received;
            }
            catch (ObjectDisposedException)
            {
                // Do nothing
            }
            catch (SocketException ex)
            {
                SendError(ex.SocketErrorCode);
                Disconnect();
            }

            return 0;
        }

        /// <summary>
        ///     Receive text from the given endpoint (synchronous).
        /// </summary>
        /// <param name="endpoint">Endpoint to receive from.</param>
        /// <param name="size">Text size to receive.</param>
        /// <returns>Received text.</returns>
        public virtual string Receive(ref EndPoint endpoint, long size)
        {
            var buffer = new byte[size];
            var length = Receive(ref endpoint, buffer);

            return Encoding.UTF8.GetString(buffer, 0, (int)length);
        }

        /// <summary>
        ///     Receive datagram from the server (asynchronous).
        /// </summary>
        public virtual void ReceiveAsync()
        {
            if (Thread.CurrentThread.ManagedThreadId == _sendThreadId)
            {
                ThreadPool.QueueUserWorkItem(_ => TryReceive());
            }
            else
            {
                TryReceive();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">A value that indicates whether the instance is disposing or finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Disconnect();

                if (_receiveEventArg != null)
                {
                    _receiveEventArg.Dispose();
                }

                if (_sendEventArg != null)
                {
                    _sendEventArg.Dispose();
                }
            }

            _disposed = true;
        }

        /// <summary>
        ///     Handle client connected notification.
        /// </summary>
        protected virtual void OnConnected()
        {
        }

        /// <summary>
        ///     Handle client disconnected notification.
        /// </summary>
        protected virtual void OnDisconnected()
        {
        }

        /// <summary>
        ///     Handle client joined multicast group notification.
        /// </summary>
        /// <param name="address">IP address</param>
        protected virtual void OnJoinedMulticastGroup(IPAddress address)
        {
        }

        /// <summary>
        /// Handle client left multicast group notification.
        /// </summary>
        /// <param name="address">IP address</param>
        protected virtual void OnLeftMulticastGroup(IPAddress address) { }

        /// <summary>
        ///     Handle datagram received notification.
        /// </summary>
        /// <param name="endpoint">Received endpoint</param>
        /// <param name="buffer">Received datagram buffer</param>
        /// <param name="offset">Received datagram buffer offset</param>
        /// <param name="size">Received datagram buffer size</param>
        /// <remarks>
        ///     Notification is called when another datagram was received from some endpoint
        /// </remarks>
        protected virtual void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
        }

        /// <summary>
        ///     Handle datagram sent notification
        /// </summary>
        /// <param name="endpoint">Endpoint of sent datagram</param>
        /// <param name="sent">Size of sent datagram buffer</param>
        /// <remarks>
        ///     Notification is called when a datagram was sent to the server.
        ///     This handler could be used to send another datagram to the server for
        ///     instance when the pending size is zero.
        /// </remarks>
        protected virtual void OnSent(EndPoint endpoint, long sent)
        {
        }

        /// <summary>
        ///     Handle error notification.
        /// </summary>
        /// <param name="error">Socket error code</param>
        protected virtual void OnError(SocketError error)
        {
        }

        /// <summary>
        ///     Try to receive new data.
        /// </summary>
        private void TryReceive()
        {
            if (_receiving)
            {
                return;
            }

            if (!IsConnected)
            {
                return;
            }

            try
            {
                _receiving = true;
                _receiveEventArg.RemoteEndPoint = _receiveEndpoint;
                _receiveEventArg.SetBuffer(_receiveBuffer.Data, 0, (int)_receiveBuffer.Capacity);

                if (Socket.ReceiveFromAsync(_receiveEventArg))
                {
                    return;
                }

                ProcessReceiveFrom(_receiveEventArg);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void TrySend()
        {
            if (_sending)
            {
                return;
            }

            if (!IsConnected)
            {
                return;
            }

            try
            {
                _sending = true;
                _sendEventArg.RemoteEndPoint = _sendEndpoint;
                _sendEventArg.SetBuffer(_sendBuffer.Data, 0, (int)_sendBuffer.Size);
                _sendThreadId = Thread.CurrentThread.ManagedThreadId;

                if (!Socket.SendToAsync(_sendEventArg))
                {
                    ProcessSendTo(_sendEventArg);
                }
            }
            catch (ObjectDisposedException) { }
        }

        private void ClearBuffers()
        {
            _sendBuffer.Clear();

            BytesPending = 0;
            BytesSending = 0;
        }

        private void OnAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    ProcessReceiveFrom(e);
                    break;
                case SocketAsyncOperation.SendTo:
                    ProcessSendTo(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void ProcessReceiveFrom(SocketAsyncEventArgs e)
        {
            _receiving = false;

            if (!IsConnected)
            {
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                SendError(e.SocketError);
                Disconnect();
            }
            else
            {
                var bytesTransferred = (long)e.BytesTransferred;

                if (bytesTransferred <= 0L)
                {
                    return;
                }

                ++DatagramsReceived;
                BytesReceived += bytesTransferred;

                OnReceived(e.RemoteEndPoint, _receiveBuffer.Data, 0L, bytesTransferred);

                if (_receiveBuffer.Capacity != bytesTransferred)
                {
                    return;
                }

                _receiveBuffer.Reserve(2L * bytesTransferred);
            }
        }

        private void ProcessSendTo(SocketAsyncEventArgs e)
        {
            _sending = false;

            if (!IsConnected)
            {
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                SendError(e.SocketError);
                Disconnect();

                return;
            }

            long sent = e.BytesTransferred;

            if (sent <= 0)
            {
                return;
            }

            BytesSending = 0;
            BytesSent += sent;

            _sendBuffer.Clear();

            OnSent(_sendEndpoint, sent);

            _sendThreadId = 0;
        }

        private void SendError(SocketError error)
        {
            if (error == SocketError.ConnectionAborted || error == SocketError.ConnectionRefused ||
                (error == SocketError.ConnectionReset || error == SocketError.OperationAborted) ||
                error == SocketError.Shutdown)
            {
                return;
            }

            OnError(error);
        }
    }
}
