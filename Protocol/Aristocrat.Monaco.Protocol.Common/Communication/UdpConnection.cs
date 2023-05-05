namespace Aristocrat.Monaco.Protocol.Common.Communication
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using log4net;
    using Monaco.Protocol.Common;

    /// <summary>
    ///     A Udp connection to receive UDP multicast messages.
    /// </summary>
    public class UdpConnection : IUdpConnection, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Subject<Packet> _incomingPayload = new Subject<Packet>();
        private readonly object _sync = new object();
        private bool _disposed;
        private UdpClient _client;
        private IPEndPoint _endPoint;

        /// <summary>
        ///     To indicate if the connection is established.
        /// </summary>
        public bool Connected
        {
            get
            {
                return (_client != null && _client.Client.Connected);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public IObservable<Packet> IncomingBytes => _incomingPayload;

        /// <inheritdoc />
        public Task<bool> Open(IPEndPoint endPoint, CancellationToken token = default)
        {
            _endPoint = endPoint;
            return AttemptConnection(token);
        }

        /// <inheritdoc />
        public Task Close(CancellationToken token = default)
        {
            return Task.Run(
                () =>
                {
                    lock (_sync)
                    {
                        try
                        {
                            if (_client == null)
                            {
                                return;
                            }

                            _client.Close();
                        }
                        finally
                        {
                            DisposeClient();
                        }
                    }
                },
                token);
        }

        /// <summary>
        ///     For cleaning up resources
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
                DisposeClient();
                _incomingPayload.Dispose();
            }

            _disposed = true;
        }

        private void ReadCallback(IAsyncResult readResult)
        {
            try
            {
                lock (_sync)
                {
                    if (_client == null)
                    {
                        Logger.Debug("Client disposed already.");
                        return;
                    }

                    var refEndPoint = new IPEndPoint(0, 0);
                    var data = _client.EndReceive(readResult, ref refEndPoint);
                    _incomingPayload.OnNext(new Packet(data, data.Length));
                    _client.BeginReceive(ReadCallback, null);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Error reading from multicast socket, it may have been closed", ex);

                // Trigger reconnection in case of an error
                if (!_disposed)
                {
                    AttemptReconnection(CancellationToken.None);
                }
            }
        }

        private Task<bool> AttemptConnection(CancellationToken token)
        {
            return Task.Run(
                () =>
                {
                    lock (_sync)
                    {
                        try
                        {
                            _client = new UdpClient();
                            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            _client.ExclusiveAddressUse = false;
                            _client.Client.Bind(new IPEndPoint(IPAddress.Any, _endPoint.Port));
                            if (!_endPoint.Address.Equals(IPAddress.Broadcast))
                            {
                                _client.JoinMulticastGroup(_endPoint.Address);
                            }
                            else
                            {
                                _client.EnableBroadcast = true;
                            }
                            _client.BeginReceive(ReadCallback, null);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Error opening multicast socket", ex);
                            DisposeClient();
                            return false;
                        }
                    }

                    return true;
                },
                token);
        }

        private async void AttemptReconnection(CancellationToken token)
        {
            Logger.Info("Attempting to reconnect...");
            int retryDelayMilliseconds = 5000;
            while (!Connected && !_disposed)
            {
                await Task.Delay(retryDelayMilliseconds, token);
                await AttemptConnection(token);
            }
        }

        private void DisposeClient()
        {
            lock (_sync)
            {
                if (_client == null)
                {
                    return;
                }

                _client.Dispose();

                _client = null;
            }
        }
    }
}