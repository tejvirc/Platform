namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using log4net;

    /// <summary>
    ///     A Udp connection to CDS server to receive progressive broadcast messages.
    /// </summary>
    public class UdpConnection : IUdpConnection, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Subject<Packet> _incomingPayload = new Subject<Packet>();
        private readonly object _sync = new object();
        private bool _disposed;
        private UdpClient _client;

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
                            _client.Client.Bind(new IPEndPoint(IPAddress.Any, endPoint.Port));
                            if (!endPoint.Address.Equals(IPAddress.Broadcast))
                            {
                                _client.JoinMulticastGroup(endPoint.Address);
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