namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using log4net;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;
    using Messages;

    /// <summary>
    ///     Implements the ITcpConnection interface using the TcpClient class
    /// </summary>
    public class TcpConnection : ITcpConnection, IDisposable
    {
        private const int ReadBufferSize = 65536;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly byte[] _readBuffer = new byte[ReadBufferSize];
        private readonly Subject<Packet> _observedBuffer = new();
        private readonly BehaviorSubject<ConnectionStatus> _statusSubject = new(new ConnectionStatus());
        private readonly SemaphoreSlim _lock = new(1);
        private readonly int _sizeOfEncryptedHeader;

        private TcpClient _transport;
        private byte[] _messageBuffer = Array.Empty<byte>();
        private MessageEncryptHeader? _header;
        private bool _disposed;

        /// <summary>
        /// 
        /// </summary>
        public TcpConnection()
        {
            _sizeOfEncryptedHeader = Marshal.SizeOf<MessageEncryptHeader>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public ConnectionStatus CurrentStatus { get; } = new ConnectionStatus();

        /// <inheritdoc />
        public IObservable<ConnectionStatus> ConnectionStatus => _statusSubject;

        /// <inheritdoc />
        public IObservable<Packet> IncomingBytes => _observedBuffer;

        /// <inheritdoc />
        public async Task<bool> Open(IPEndPoint endPoint, CancellationToken token = default)
        {
            await _lock.WaitAsync(token);
            try
            {
                // If we are already connected, ignore this request.
                if (CurrentStatus.ConnectionState == ConnectionState.Connected)
                {
                    return true;
                }

                // Otherwise, create a fresh socket and connect it to the endpoint.
                _messageBuffer = Array.Empty<byte>();
                _header = null;

                _transport = new TcpClient();
                _transport?.Connect(endPoint);
                // This will wait (without blocking) until we either get some bytes or the connection is closed.
                _transport?.GetStream().BeginRead(_readBuffer, 0, ReadBufferSize, ReadCallback, null);
                SetConnectionStatus(ConnectionState.Connected);
            }
            catch (Exception ex)
            {
                Logger.Error("Connection failed!", ex);
                // Any failure, including cancelling the task, should leave us in a disconnected state.
                SetConnectionStatus(ConnectionState.Disconnected);
                return false;
            }
            finally
            {
                _lock.Release();
            }

            return true;
        }

        /// <inheritdoc />
        public async Task Close(CancellationToken token = default)
        {
            if (_disposed)
            {
                return;
            }
            
            await _lock.WaitAsync(token);

            CloseInternal();

            _lock.Release();
        }

        /// <inheritdoc />
        public async Task<bool> SendBytes(byte[] message, CancellationToken token = default)
        {
            await _lock.WaitAsync(token);

            if (CurrentStatus.ConnectionState != ConnectionState.Connected)
            {
                _lock.Release();
                return false;
            }

            try
            {
                await _transport?.GetStream().WriteAsync(message, 0, message.Length, token);

                return true;
            }
            catch (Exception exception)
            {
                Logger.Error("Unable to send data to server.", exception);
                // This will result in a task running that we don't wait for and don't care what happens
                CloseInternal();
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _observedBuffer.Dispose();
                _transport?.Dispose();
                _statusSubject.Dispose();

                // ReSharper disable once UseNullPropagation
                if (_lock != null)
                {
                    _lock.Dispose();
                }
            }

            _transport = null;

            _disposed = true;
            
        }

        private async void ReadCallback(IAsyncResult readResult)
        {
            // Happens when we dispose before closing.
            if (_disposed)
            {
                return;
            }

            // Complete the read operation by finding out how many bytes we received while waiting.
            await _lock.WaitAsync();
            {
                try
                {
                    var bytesRead = _transport?.GetStream().EndRead(readResult) ?? 0;

                    if (bytesRead != 0)
                    {
                        OnDataReceived(bytesRead);
                        _transport?.GetStream().BeginRead(_readBuffer, 0, ReadBufferSize, ReadCallback, null);
                    }
                    else
                    {
                        Logger.Warn(
                            "The socket read returned zero bytes, the remote server has closed the connection.");
                        CloseInternal();
                    }
                }
                catch (Exception exception)
                {
                    Logger.Warn(
                        "The socket read threw an exception, the remote server cannot be contacted.",
                        exception);
                    CloseInternal();
                }
                finally
                {
                    _lock.Release();
                }
            }
        }

        private void OnDataReceived(int bytesRead)
        {
            AddPacketIntoBuffer(bytesRead);

            while (_messageBuffer.Length >= _sizeOfEncryptedHeader && IsPacketComplete())
            {
                _observedBuffer.OnNext(new Packet(_messageBuffer, _header?.EncryptionLength ?? 0));
                NextPacket();
            }
        }

        private void NextPacket()
        {
            var previousPacketLength = _header?.EncryptionLength ?? 0;
            var buffer = new byte[_messageBuffer.Length - _header?.EncryptionLength ?? 0];
            Array.Copy(_messageBuffer, previousPacketLength, buffer, 0, _messageBuffer.Length - previousPacketLength);
            _messageBuffer = buffer;
            _header = null;
        }

        private void AddPacketIntoBuffer(int bytesRead)
        {
            var length = _messageBuffer.Length;
            Array.Resize(ref _messageBuffer, bytesRead + length);
            Array.Copy(_readBuffer, 0, _messageBuffer, length, bytesRead);
        }

        private bool IsPacketComplete()
        {
            if (_header == null)
            {
                _header = MessageUtility.ConvertByteArrayToMessage<MessageEncryptHeader>(_messageBuffer);
            }

            return _messageBuffer.Length >= _header?.EncryptionLength;
        }

        private void SetConnectionStatus(ConnectionState newState)
        {
            Logger.Debug($"Connection state changed : {CurrentStatus.ConnectionState} to {newState}");
            CurrentStatus.ConnectionState = newState;
            _statusSubject.OnNext(CurrentStatus);
        }

        private void CloseInternal()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                SetConnectionStatus(ConnectionState.Disconnected);
                _transport?.Close();
                _transport = null;
            }
            catch (Exception e)
            {
                Logger.Debug("Failed to close", e);
            }
        }
    }
}