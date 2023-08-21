namespace Aristocrat.Monaco.Protocol.Common.Communication
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     SSL client is used to read/write data from/into the connected SSL server
    /// </summary>
    public class SslClient : IDisposable
    {
        private bool _receiving;
        private Buffer _receiveBuffer;
        private readonly object _sendLock = new object();
        private bool _sending;
        private Buffer _sendBufferMain;
        private Buffer _sendBufferFlush;
        private long _sendBufferFlushOffset;

        private bool _disposed;

        /// <summary>
        ///     Initialize SSL client with a given server IP address and port number
        /// </summary>
        /// <param name="context">SSL context</param>
        /// <param name="address">IP address</param>
        /// <param name="port">Port number</param>
        public SslClient(SslContext context, IPAddress address, int port)
            : this(context, new IPEndPoint(address, port))
        {
        }

        /// <summary>
        ///     Initialize SSL client with a given server IP address and port number
        /// </summary>
        /// <param name="context">SSL context</param>
        /// <param name="address">IP address</param>
        /// <param name="port">Port number</param>
        public SslClient(SslContext context, string address, int port)
            : this(context, new IPEndPoint(IPAddress.Parse(address), port))
        {
            Address = address;
        }

        /// <summary>
        ///     Initialize SSL client with a given IP endpoint
        /// </summary>
        /// <param name="context">SSL context</param>
        /// <param name="endpoint">IP endpoint</param>
        public SslClient(SslContext context, IPEndPoint endpoint)
        {
            Id = Guid.NewGuid();
            Address = endpoint.Address.ToString();
            Context = context;
            Endpoint = endpoint;
        }

        /// <inheritdoc />
        ~SslClient()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Client Id
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     SSL server DNS address
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        ///     SSL context
        /// </summary>
        public SslContext Context { get; }
        /// <summary>
        ///     IP endpoint
        /// </summary>
        public IPEndPoint Endpoint { get; }
        /// <summary>
        ///     Socket
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        ///     Number of bytes pending sent by the client
        /// </summary>
        public long BytesPending { get; private set; }
        /// <summary>
        ///     Number of bytes sending by the client
        /// </summary>
        public long BytesSending { get; private set; }
        /// <summary>
        ///     Number of bytes sent by the client
        /// </summary>
        public long BytesSent { get; private set; }
        /// <summary>
        ///     Number of bytes received by the client
        /// </summary>
        public long BytesReceived { get; private set; }

        /// <summary>
        ///     Option: keep alive
        /// </summary>
        /// <remarks>
        ///     This option will setup SO_KEEP-ALIVE if the OS support this feature
        /// </remarks>
        public bool OptionKeepAlive { get; set; }
        /// <summary>
        ///     Option: no delay
        /// </summary>
        /// <remarks>
        ///     This option will enable/disable algorithm for SSL protocol
        /// </remarks>
        public bool OptionNoDelay { get; set; }
        /// <summary>
        ///     Option: receive buffer size
        /// </summary>
        public int OptionReceiveBufferSize { get; set; } = 8192;
        /// <summary>
        ///     Option: send buffer size
        /// </summary>
        public int OptionSendBufferSize { get; set; } = 8192;

        private bool _disconnecting;
        private SocketAsyncEventArgs _connectEventArg;
        private SslStream _sslStream;
        private Guid? _sslStreamId;

        /// <summary>
        ///     Is the client connecting?
        /// </summary>
        public bool IsConnecting { get; private set; }
        /// <summary>
        ///     Is the client connected?
        /// </summary>
        public bool IsConnected { get; private set; }
        /// <summary>
        ///     Is the client handshaking?
        /// </summary>
        public bool IsHandshaking { get; private set; }
        /// <summary>
        ///     Is the client handshaked?
        /// </summary>
        public bool IsHandshaked { get; private set; }

        /// <summary>
        ///     Connect the client (synchronous)
        /// </summary>
        /// <remarks>
        ///     Please note that synchronous connect will not receive data automatically!
        ///     You should use Receive() or ReceiveAsync() method manually after successful connection.
        /// </remarks>
        /// <returns>'true' if the client was successfully connected, 'false' if the client failed to connect</returns>
        public virtual bool Connect()
        {
            if (IsConnected || IsHandshaked || IsConnecting || IsHandshaking)
                return false;

            // Setup buffers
            _receiveBuffer = new Buffer();
            _sendBufferMain = new Buffer();
            _sendBufferFlush = new Buffer();

            // Setup event args
            _connectEventArg = new SocketAsyncEventArgs { RemoteEndPoint = Endpoint };
            _connectEventArg.Completed += OnAsyncCompleted;

            // Create a new client socket
            Socket = new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Connect to the server
                Socket.Connect(Endpoint);
            }
            catch (SocketException ex)
            {
                // Close the client socket
                Socket.Close();
                // Dispose the client socket
                Socket.Dispose();

                // Call the client disconnected handler
                SendError(ex.SocketErrorCode);
                OnDisconnected();
                return false;
            }

            // Update the client socket disposed flag
            IsSocketDisposed = false;

            // Apply the option: keep alive
            if (OptionKeepAlive)
                Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            // Apply the option: no delay
            if (OptionNoDelay)
                Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

            // Prepare receive & send buffers
            _receiveBuffer.Reserve(OptionReceiveBufferSize);
            _sendBufferMain.Reserve(OptionSendBufferSize);
            _sendBufferFlush.Reserve(OptionSendBufferSize);

            // Reset statistic
            BytesPending = 0;
            BytesSending = 0;
            BytesSent = 0;
            BytesReceived = 0;

            // Update the connected flag
            IsConnected = true;

            // Call the client connected handler
            OnConnected();

            try
            {
                // Create SSL stream
                _sslStreamId = Guid.NewGuid();
                _sslStream = Context.CertificateValidationCallback != null ? new SslStream(new NetworkStream(Socket, false), false, Context.CertificateValidationCallback) : new SslStream(new NetworkStream(Socket, false), false);

                // SSL handshake
                if (Context.Certificates == null && Context.Certificate == null)
                {
                    _sslStream.AuthenticateAsClient(Address);
                }
                else
                {
                    _sslStream.AuthenticateAsClient(Address, Context.Certificates ?? new X509CertificateCollection(new[] { Context.Certificate }), Context.Protocols, true);
                }
            }
            catch (Exception ex)
            {
                SendError(ex);
                DisconnectAsync();
                return false;
            }

            // Update the handshaked flag
            IsHandshaked = true;

            // Call the session handshaked handler
            OnHandshaked();

            // Call the empty send buffer handler
            if (_sendBufferMain.IsEmpty)
                OnEmpty();

            return true;
        }

        /// <summary>
        ///     Disconnect the client (synchronous)
        /// </summary>
        /// <returns>'true' if the client was successfully disconnected, 'false' if the client is already disconnected</returns>
        public virtual bool Disconnect()
        {
            if (!IsConnected && !IsConnecting)
            {
                return false;
            }

            // Cancel connecting operation
            if (IsConnecting)
            {
                Socket.CancelConnectAsync(_connectEventArg);
            }

            if (_disconnecting)
            {
                return false;
            }

            // Reset connecting & handshaking flags
            IsConnecting = false;
            IsHandshaking = false;

            // Update the disconnecting flag
            _disconnecting = true;

            // Reset event args
            _connectEventArg.Completed -= OnAsyncCompleted;

            try
            {
                try
                {
                    // Shutdown the SSL stream
                    _sslStream.ShutdownAsync().Wait();
                }
                catch (Exception ex)
                {
                    SendError(ex);
                }

                // Dispose the SSL stream & buffer
                _sslStream.Dispose();
                _sslStreamId = null;

                try
                {
                    // Shutdown the socket associated with the client
                    Socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException)
                {
                }

                // Close the client socket
                Socket.Close();

                // Dispose the client socket
                Socket.Dispose();

                // Update the client socket disposed flag
                IsSocketDisposed = true;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                SendError(ex);
            }

            // Update the handshaked flag
            IsHandshaked = false;

            // Update the connected flag
            IsConnected = false;

            // Update sending/receiving flags
            _receiving = false;
            _sending = false;

            // Clear send/receive buffers
            ClearBuffers();

            // Call the client disconnected handler
            OnDisconnected();

            // Reset the disconnecting flag
            _disconnecting = false;

            return true;
        }

        /// <summary>
        ///     Reconnect the client (synchronous)
        /// </summary>
        /// <returns>'true' if the client was successfully reconnected, 'false' if the client is already reconnected</returns>
        public virtual bool Reconnect()
        {
            if (!Disconnect())
            {
                return false;
            }

            return Connect();
        }

        /// <summary>
        ///     Connect the client (asynchronous)
        /// </summary>
        /// <returns>'true' if the client was successfully connected, 'false' if the client failed to connect</returns>
        public virtual bool ConnectAsync()
        {
            if (IsConnected || IsHandshaked || IsConnecting || IsHandshaking)
            {
                return false;
            }

            // Setup buffers
            _receiveBuffer = new Buffer();
            _sendBufferMain = new Buffer();
            _sendBufferFlush = new Buffer();

            // Setup event args
            _connectEventArg = new SocketAsyncEventArgs { RemoteEndPoint = Endpoint };
            _connectEventArg.Completed += OnAsyncCompleted;

            // Create a new client socket
            Socket = new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Async connect to the server
            IsConnecting = true;
            if (!Socket.ConnectAsync(_connectEventArg))
            {
                ProcessConnect(_connectEventArg);
            }

            return true;
        }

        /// <summary>
        ///     Disconnect the client (asynchronous)
        /// </summary>
        /// <returns>'true' if the client was successfully disconnected, 'false' if the client is already disconnected</returns>
        public virtual bool DisconnectAsync() { return Disconnect(); }

        /// <summary>
        ///     Reconnect the client (asynchronous)
        /// </summary>
        /// <returns>'true' if the client was successfully reconnected, 'false' if the client is already reconnected</returns>
        public virtual bool ReconnectAsync()
        {
            if (!DisconnectAsync())
            {
                return false;
            }

            while (IsConnected)
            {
                Thread.Yield();
            }

            return ConnectAsync();
        }

        /// <summary>
        ///     Send data to the server (synchronous)
        /// </summary>
        /// <param name="buffer">Buffer to send</param>
        /// <returns>Size of sent data</returns>
        public virtual long Send(byte[] buffer) { return Send(buffer, 0, buffer.Length); }

        /// <summary>
        ///     Send data to the server (synchronous)
        /// </summary>
        /// <param name="buffer">Buffer to send</param>
        /// <param name="offset">Buffer offset</param>
        /// <param name="size">Buffer size</param>
        /// <returns>Size of sent data</returns>
        public virtual long Send(byte[] buffer, long offset, long size)
        {
            if (!IsHandshaked)
            {
                return 0;
            }

            if (size == 0)
            {
                return 0;
            }

            try
            {
                // Sent data to the server
                _sslStream.Write(buffer, (int)offset, (int)size);

                // Update statistic
                BytesSent += size;

                // Call the buffer sent handler
                OnSent(size, BytesPending + BytesSending);

                return size;
            }
            catch (Exception ex)
            {
                SendError(ex);
                Disconnect();
                return 0;
            }
        }

        /// <summary>
        ///     Send text to the server (synchronous)
        /// </summary>
        /// <param name="text">Text string to send</param>
        /// <returns>Size of sent text</returns>
        public virtual long Send(string text) { return Send(Encoding.UTF8.GetBytes(text)); }

        /// <summary>
        ///     Send data to the server (asynchronous)
        /// </summary>
        /// <param name="buffer">Buffer to send</param>
        /// <returns>'true' if the data was successfully sent, 'false' if the client is not connected</returns>
        public virtual bool SendAsync(byte[] buffer) { return SendAsync(buffer, 0, buffer.Length); }

        /// <summary>
        ///     Send data to the server (asynchronous)
        /// </summary>
        /// <param name="buffer">Buffer to send</param>
        /// <param name="offset">Buffer offset</param>
        /// <param name="size">Buffer size</param>
        /// <returns>'true' if the data was successfully sent, 'false' if the client is not connected</returns>
        public virtual bool SendAsync(byte[] buffer, long offset, long size)
        {
            if (!IsHandshaked)
            {
                return false;
            }

            if (size == 0)
            {
                return true;
            }

            lock (_sendLock)
            {
                // Detect multiple send handlers
                var sendRequired = _sendBufferMain.IsEmpty || _sendBufferFlush.IsEmpty;

                // Fill the main send buffer
                _sendBufferMain.Append(buffer, offset, size);

                // Update statistic
                BytesPending = _sendBufferMain.Size;

                // Avoid multiple send handlers
                if (!sendRequired)
                {
                    return true;
                }
            }

            // Try to send the main buffer
            Task.Factory.StartNew(TrySend);

            return true;
        }

        /// <summary>
        ///     Send text to the server (asynchronous)
        /// </summary>
        /// <param name="text">Text string to send</param>
        /// <returns>'true' if the text was successfully sent, 'false' if the client is not connected</returns>
        public virtual bool SendAsync(string text) { return SendAsync(Encoding.UTF8.GetBytes(text)); }

        /// <summary>
        ///     Receive data from the server (synchronous)
        /// </summary>
        /// <param name="buffer">Buffer to receive</param>
        /// <returns>Size of received data</returns>
        public virtual long Receive(byte[] buffer) { return Receive(buffer, 0, buffer.Length); }

        /// <summary>
        ///     Receive data from the server (synchronous)
        /// </summary>
        /// <param name="buffer">Buffer to receive</param>
        /// <param name="offset">Buffer offset</param>
        /// <param name="size">Buffer size</param>
        /// <returns>Size of received data</returns>
        public virtual long Receive(byte[] buffer, long offset, long size)
        {
            if (!IsHandshaked)
            {
                return 0;
            }

            if (size == 0)
            {
                return 0;
            }

            try
            {
                // Receive data from the server
                long received = _sslStream.Read(buffer, (int)offset, (int)size);
                if (received > 0)
                {
                    // Update statistic
                    BytesReceived += received;

                    // Call the buffer received handler
                    OnReceived(buffer, 0, received);
                }

                return received;
            }
            catch (Exception ex)
            {
                SendError(ex);
                Disconnect();
                return 0;
            }
        }

        /// <summary>
        ///     Receive text from the server (synchronous)
        /// </summary>
        /// <param name="size">Text size to receive</param>
        /// <returns>Received text</returns>
        public virtual string Receive(long size)
        {
            var buffer = new byte[size];
            var length = Receive(buffer);
            return Encoding.UTF8.GetString(buffer, 0, (int)length);
        }

        /// <summary>
        ///     Receive data from the server (asynchronous)
        /// </summary>
        public virtual void ReceiveAsync()
        {
            // Try to receive data from the server
            TryReceive();
        }

        /// <summary>
        /// Try to receive new data
        /// </summary>
        private void TryReceive()
        {
            if (_receiving)
            {
                return;
            }

            if (!IsHandshaked)
            {
                return;
            }

            try
            {
                // Async receive with the receive handler
                IAsyncResult result;
                do
                {
                    if (!IsHandshaked)
                    {
                        return;
                    }

                    _receiving = true;
                    result = _sslStream.BeginRead(
                        _receiveBuffer.Data,
                        0,
                        (int)_receiveBuffer.Capacity,
                        ProcessReceive,
                        _sslStreamId);
                } while (result.CompletedSynchronously);

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                SendError(ex);
            }
        }

        private void TrySend()
        {
            if (_sending)
            {
                return;
            }

            if (!IsHandshaked)
            {
                return;
            }

            lock (_sendLock)
            {
                if (_sending)
                {
                    return;
                }

                // Swap send buffers
                if (_sendBufferFlush.IsEmpty)
                {
                    lock (_sendLock)
                    {
                        // Swap flush and main buffers
                        _sendBufferFlush = Interlocked.Exchange(ref _sendBufferMain, _sendBufferFlush);
                        _sendBufferFlushOffset = 0;

                        // Update statistic
                        BytesPending = 0;
                        BytesSending += _sendBufferFlush.Size;

                        _sending = !_sendBufferFlush.IsEmpty;
                    }
                }
                else
                {
                    return;
                }
            }

            // Check if the flush buffer is empty
            if (_sendBufferFlush.IsEmpty)
            {
                // Call the empty send buffer handler
                OnEmpty();
                return;
            }

            try
            {
                // Async write with the write handler
                _sslStream.BeginWrite(
                    _sendBufferFlush.Data,
                    (int)_sendBufferFlushOffset,
                    (int)(_sendBufferFlush.Size - _sendBufferFlushOffset),
                    ProcessSend,
                    _sslStreamId);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                SendError(ex);
            }
        }

        private void ClearBuffers()
        {
            lock (_sendLock)
            {
                // Clear send buffers
                _sendBufferMain.Clear();
                _sendBufferFlush.Clear();
                _sendBufferFlushOffset= 0;

                // Update statistic
                BytesPending = 0;
                BytesSending = 0;
            }
        }

        private void OnAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            // Determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            IsConnecting = false;

            if (e.SocketError == SocketError.Success)
            {
                // Apply the option: keep alive
                if (OptionKeepAlive)
                {
                    Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                }

                // Apply the option: no delay
                if (OptionNoDelay)
                {
                    Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                }

                // Prepare receive & send buffers
                _receiveBuffer.Reserve(OptionReceiveBufferSize);
                _sendBufferMain.Reserve(OptionSendBufferSize);
                _sendBufferFlush.Reserve(OptionSendBufferSize);

                // Reset statistic
                BytesPending = 0;
                BytesSending = 0;
                BytesSent = 0;
                BytesReceived = 0;

                // Update the connected flag
                IsConnected = true;

                // Call the client connected handler
                OnConnected();

                try
                {
                    // Create SSL stream
                    _sslStreamId = Guid.NewGuid();
                    _sslStream = Context.CertificateValidationCallback != null ? new SslStream(new NetworkStream(Socket, false), false, Context.CertificateValidationCallback) : new SslStream(new NetworkStream(Socket, false), false);

                    // Begin the SSL handshake
                    IsHandshaking = true;
                    if (Context.Certificates == null && Context.Certificate == null)
                    {
                        _sslStream.BeginAuthenticateAsClient(Address, ProcessHandshake, _sslStreamId);
                    }
                    else
                    {
                        _sslStream.BeginAuthenticateAsClient(Address, Context.Certificates ?? new X509CertificateCollection(new[] { Context.Certificate }), Context.Protocols, true, ProcessHandshake, _sslStreamId);
                    }
                }
                catch (Exception ex)
                {
                    SendError(ex);
                    DisconnectAsync();
                }
            }
            else
            {
                // Call the client disconnected handler
                SendError(e.SocketError);
                OnDisconnected();
            }
        }

        private void ProcessHandshake(IAsyncResult result)
        {
            try
            {
                IsHandshaking = false;

                if (IsHandshaked)
                {
                    return;
                }

                // Validate SSL stream Id
                var sslStreamId = result.AsyncState as Guid?;
                if (_sslStreamId != sslStreamId)
                {
                    return;
                }

                // End the SSL handshake
                _sslStream.EndAuthenticateAsClient(result);

                // Update the handshaked flag
                IsHandshaked = true;

                // Call the session handshaked handler
                OnHandshaked();

                // Call the empty send buffer handler
                if (_sendBufferMain.IsEmpty)
                {
                    OnEmpty();
                }

                // Try to receive something from the server
                TryReceive();
            }
            catch (Exception ex)
            {
                SendError(ex);
                DisconnectAsync();
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous receive operation completes
        /// </summary>
        private void ProcessReceive(IAsyncResult result)
        {
            try
            {
                if (!IsHandshaked)
                {
                    return;
                }

                // Validate SSL stream Id
                var sslStreamId = result.AsyncState as Guid?;
                if (_sslStreamId != sslStreamId)
                {
                    return;
                }

                // End the SSL read
                long size = _sslStream.EndRead(result);

                // Received some data from the server
                if (size > 0)
                {
                    // Update statistic
                    BytesReceived += size;

                    // Call the buffer received handler
                    OnReceived(_receiveBuffer.Data, 0, size);

                    // If the receive buffer is full increase its size
                    if (_receiveBuffer.Capacity == size)
                    {
                        _receiveBuffer.Reserve(2 * size);
                    }
                }

                _receiving = false;

                // If zero is returned from a read operation, the remote end has closed the connection
                if (size > 0)
                {
                    TryReceive();
                }
                else
                {
                    DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                SendError(ex);
                DisconnectAsync();
            }
        }

        private void ProcessSend(IAsyncResult result)
        {
            try
            {
                if (!IsHandshaked)
                {
                    return;
                }

                // Validate SSL stream Id
                var sslStreamId = result.AsyncState as Guid?;
                if (_sslStreamId != sslStreamId)
                {
                    return;
                }

                // End the SSL write
                _sslStream.EndWrite(result);

                var size = _sendBufferFlush.Size;

                // Send some data to the server
                if (size > 0)
                {
                    // Update statistic
                    BytesSending -= size;
                    BytesSent += size;

                    // Increase the flush buffer offset
                    _sendBufferFlushOffset += size;

                    // Successfully send the whole flush buffer
                    if (_sendBufferFlushOffset == _sendBufferFlush.Size)
                    {
                        // Clear the flush buffer
                        _sendBufferFlush.Clear();
                        _sendBufferFlushOffset = 0;
                    }

                    // Call the buffer sent handler
                    OnSent(size, BytesPending + BytesSending);
                }

                _sending = false;

                // Try to send again if the client is valid
                TrySend();
            }
            catch (Exception ex)
            {
                SendError(ex);
                DisconnectAsync();
            }
        }

        /// <summary>
        ///     Handle client connected notification
        /// </summary>
        protected virtual void OnConnected() {}
        /// <summary>
        ///     Handle client handshaked notification
        /// </summary>
        protected virtual void OnHandshaked() {}
        /// <summary>
        ///     Handle client disconnected notification
        /// </summary>
        protected virtual void OnDisconnected() {}

        /// <summary>
        ///     Handle buffer received notification
        /// </summary>
        /// <param name="buffer">Received buffer</param>
        /// <param name="offset">Received buffer offset</param>
        /// <param name="size">Received buffer size</param>
        /// <remarks>
        /// Notification is called when another chunk of buffer was received from the server
        /// </remarks>
        protected virtual void OnReceived(byte[] buffer, long offset, long size) {}

        /// <summary>
        ///     Handle buffer sent notification
        /// </summary>
        /// <param name="sent">Size of sent buffer</param>
        /// <param name="pending">Size of pending buffer</param>
        /// <remarks>
        /// Notification is called when another chunk of buffer was sent to the server.
        /// This handler could be used to send another buffer to the server for instance when the pending size is zero.
        /// </remarks>
        protected virtual void OnSent(long sent, long pending) {}

        /// <summary>
        ///     Handle empty send buffer notification
        /// </summary>
        /// <remarks>
        /// Notification is called when the send buffer is empty and ready for a new data to send.
        /// This handler could be used to send another buffer to the server.
        /// </remarks>
        protected virtual void OnEmpty() {}

        /// <summary>
        ///     Handle error notification
        /// </summary>
        /// <param name="exception">Error exception</param>
        protected virtual void OnError(Exception exception) {}

        private void SendError(Exception exception)
        {
            OnError(exception);
        }

        private void SendError(SocketError error)
        {
            // Skip disconnect errors
            if (error == SocketError.ConnectionAborted ||
                error == SocketError.ConnectionRefused ||
                error == SocketError.ConnectionReset ||
                error == SocketError.OperationAborted ||
                error == SocketError.Shutdown)
                return;

            SendError(new SocketException((int)error));
        }

        /// <summary>
        ///     Client socket disposed flag
        /// </summary>
        public bool IsSocketDisposed { get; private set; } = true;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisconnectAsync();
                }

                if (_connectEventArg != null)
                {
                    _connectEventArg.Dispose();
                    _connectEventArg = null;
                }

                _disposed = true;
            }
        }
    }
}
