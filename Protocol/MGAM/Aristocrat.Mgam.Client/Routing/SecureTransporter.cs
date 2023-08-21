namespace Aristocrat.Mgam.Client.Routing
{
    using Attribute;
    using Client;
    using Common;
    using Helpers;
    using Logging;
    using Messaging;
    using Monaco.Common;
    using Monaco.Protocol.Common.Communication;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Options;

    /// <summary>
    ///     Transports messages to the host over SSL TCP sockets.
    /// </summary>
    internal sealed class SecureTransporter : ISecureTransporter, IStartable, IDisposable
    {
        private const int ClientLockTimeout = 20;

        private readonly ILogger _logger;
        private readonly IOptionsMonitor<ProtocolOptions> _options;
        private readonly IAttributeCache _attributes;
        private readonly IClientFactory _clientFactory;
        private readonly ITransportPublisher _publisher;

        private readonly IObservable<EventPattern<PayloadEventArgs>> _messages;

        private ManualResetEvent _connected = new(false);

        private Timer _idleTimer;

        private SemaphoreSlim _clientLock = new(1);

        private ISecureClient _client;

        private SubscriptionList _subscriptions = new();

        private CancellationTokenSource _shutdown = new();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BroadcastTransporter"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/></param>
        /// <param name="options"><see cref="IOptionsMonitor{TOptions}"/></param>
        /// <param name="attributes"><see cref="IAttributeCache"/>.</param>
        /// <param name="clientFactory"><see cref="IClientFactory"/>.</param>
        /// <param name="publisher"><see cref="ITransportPublisher"/>.</param>
        public SecureTransporter(
            ILogger<SecureTransporter> logger,
            IOptionsMonitor<ProtocolOptions> options,
            IAttributeCache attributes,
            IClientFactory clientFactory,
            ITransportPublisher publisher)
        {
            _logger = logger;
            _options = options;
            _attributes = attributes;
            _clientFactory = clientFactory;
            _publisher = publisher;

            _messages = Observable.FromEventPattern<PayloadEventArgs>(
                h => MessageReceived += h,
                h => MessageReceived -= h);

            _idleTimer = new Timer(_ => OnIdleTick(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            _subscriptions.Add(
                _attributes.Subscribe(
                    AttributeNames.KeepAliveInterval,
                    Observer.Create<int>(
                        OnKeepAliveChanged,
                        ex => _logger.LogError(ex, "Subscribe to keep-alive changes failure."))));
        }

        /// <inheritdoc />
        ~SecureTransporter()
        {
            Dispose(false);
        }

        private event EventHandler<PayloadEventArgs> MessageReceived;

        /// <inheritdoc />
        public IPEndPoint EndPoint => _client?.Endpoint;

        /// <inheritdoc />
        public IObservable<Payload?> Messages => _messages.Select(e => e.EventArgs?.Payload).AsObservable();

        /// <inheritdoc />
        public Task Send(Payload payload, CancellationToken cancellationToken)
        {
            if (_shutdown.IsCancellationRequested)
            {
                throw new InvalidOperationException("Shutdown requested");
            }

            InvokeClient(
                client =>
                {
                    _logger.LogDebug(
                        "Sending {0} message of size {1} to {2}",
                        payload.Format,
                        payload.MessageSize,
                        client.Endpoint);

                    client.SendAsync(payload.Content);

                    RestartIdleTimer();
                });

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task Start()
        {
            _logger.LogInfo($"Starting {GetType().Name}...");

            _logger.LogInfo($"Started {GetType().Name}...");

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool CanStart() => true;

        /// <inheritdoc />
        public Task Stop()
        {
            _logger.LogInfo($"Stopping {GetType().Name}...");

            _shutdown.Cancel();

            _logger.LogInfo($"Stopped {GetType().Name}...");

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task Connect(IPEndPoint endPoint, CancellationToken cancellationToken)
        {
            ConnectClient(endPoint);
            await _connected.AsTask(cancellationToken);
        }

        /// <inheritdoc />
        public Task Disconnect(CancellationToken cancellationToken)
        {
            DisconnectClient();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ReSharper disable once UseNullPropagation
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_client != null)
                {
                    _client.Connected -= OnConnected;
                    _client.Disconnected -= OnDisconnected;
                    _client.Received -= OnReceived;
                    _client.Error -= OnError;

                    _client.Dispose();
                }

                if (_subscriptions != null)
                {
                    _subscriptions.Dispose();
                }

                if (_clientLock != null)
                {
                    _clientLock.Dispose();
                }

                if (_idleTimer != null)
                {
                    _idleTimer.Dispose();
                }

                if (_connected != null)
                {
                    _connected.Dispose();
                }

                if (_shutdown != null)
                {
                    _shutdown.Dispose();
                }
            }

            _subscriptions = null;
            _client = null;
            _clientLock = null;
            _idleTimer = null;
            _connected = null;
            _shutdown = null;

            _disposed = true;
        }

        private void ConnectClient(IPEndPoint endPoint)
        {
            if (!_clientLock.Wait(TimeSpan.FromSeconds(ClientLockTimeout)))
            {
                throw new InvalidOperationException("Connection is locked");
            }

            try
            {
                if (_client is { IsConnected: true })
                {
                    _logger.LogInfo($"Connection already open to {_client.Endpoint.Address}");
                    return;
                }

                _logger.LogInfo("Connecting to {0}", endPoint.Address.ToString());

                var context =
                    new SslContext { CertificateValidationCallback = ValidateCertificate };

                _client = _clientFactory.CreateSecureClient(context, endPoint);

                _client.OptionKeepAlive = true;
                _client.OptionNoDelay = true;

                _client.Connected += OnConnected;
                _client.Disconnected += OnDisconnected;
                _client.Received += OnReceived;
                _client.Error += OnError;

                if (!_client.IsConnected)
                {
                    _client.ConnectAsync();
                }
            }
            finally
            {
                _clientLock.Release();
            }
        }

        private void DisconnectClient()
        {
            if (!_clientLock.Wait(TimeSpan.FromSeconds(ClientLockTimeout)))
            {
                throw new InvalidOperationException("Connection is locked");
            }

            try
            {
                if (_client == null || !_client.IsConnected)
                {
                    _logger.LogInfo("Connection already closed");
                    return;
                }

                _logger.LogInfo("Disconnecting from {0}", _client.Endpoint.Address.ToString());

                _client.DisconnectAsync();
            }
            finally
            {
                if (_client != null)
                {
                    _client.Connected -= OnConnected;
                    _client.Disconnected -= OnDisconnected;
                    _client.Received -= OnReceived;
                    _client.Error -= OnError;

                    _client = null;
                }

                _clientLock.Release();
            }
        }

        private bool ValidateCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors errors)
        {
            if (!_options.CurrentValue.ServerCertificateValidation)
            {
                _logger.LogError("Server certificate validation is disabled");
                return true;
            }

            if (certificate == null)
            {
                _logger.LogError("Certificate was not provided by the server");
                return false;
            }

            var cert = new X509Certificate2(certificate.GetRawCertData());

            _logger.LogInfo(
                "Validating server cert from Issuer='{0}', Subject='{1}', Serial Number={2}",
                cert.Issuer,
                cert.Subject,
                cert.SerialNumber);

            if (errors != SslPolicyErrors.None && errors != SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                _logger.LogError("The certificate ({0} has errors: {1}", cert.SerialNumber, string.Join(",", errors));
                return false;
            }

            if (chain.ChainStatus.Any(
                s => (s.Status & X509ChainStatusFlags.UntrustedRoot) == X509ChainStatusFlags.UntrustedRoot))
            {
                _logger.LogError("The certificate ({0} has an untrusted root", cert.SerialNumber);

                return false;
            }

            return true;
        }

        private void InvokeClient(Action<ISecureClient> action)
        {
            var client = _client;
            if (client == null || !_connected.WaitOne(TimeSpan.Zero))
            {
                throw new InvalidOperationException("No Connection");
            }

            action.Invoke(client);
        }

        private void OnConnected(object sender, ConnectedEventArgs args)
        {
            _logger.LogInfo("TCP client connected at {0}", args.EndPoint);

            _connected.Set();

            RestartIdleTimer();

            _publisher.Publish(new TransportStatus
            (
                TransportState.Up,
                false,
                args.EndPoint,
                ConnectionState.Connected
            ));
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs args)
        {
            _logger.LogWarn("TCP client disconnected at {0}", args.EndPoint);

            var wasConnected = _connected.WaitOne(TimeSpan.Zero);

            _connected.Reset();

            StopIdleTimer();

            if (wasConnected)
            {
                _publisher.Publish(new TransportStatus
                (
                    TransportState.Down,
                    false,
                    args.EndPoint,
                    ConnectionState.Lost
                ));
            }
        }

        private void OnReceived(object sender, ReceivedEventArgs args)
        {
            _logger.LogDebug(
                "TCP client received message at {0} of size {1}, Thread: {2}",
                args.EndPoint,
                args.Size,
                Thread.CurrentThread.ManagedThreadId);

            try
            {
                var payload = MessageHelper.ParseResponse(args.Buffer);

                payload.IsBroadcast = false;
                payload.EndPoint = args.EndPoint;

                RaiseMessageReceived(payload);
            }
            catch (Exception ex)
            {
                _publisher.Publish(
                    new TransportStatus(
                        TransportState.Up,
                        false,
                        EndPoint,
                        ConnectionState.Unchanged,
                        TransportFailure.Malformed));

                _logger.LogError(ex, "TCP client received malformed message at {0}", args.EndPoint);
            }
        }

        private void OnError(object sender, ErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "TCP client error at {0}", args.EndPoint);
        }

        private void OnIdleTick()
        {
            if (!_connected.WaitOne(TimeSpan.Zero))
            {
                return;
            }

            _logger.LogDebug("TCP client is idle at {0}", _client?.Endpoint);

            _publisher.Publish(new TransportStatus
            (
                TransportState.Up,
                false,
                _client?.Endpoint,
                ConnectionState.Idle
            ));
        }

        private void OnKeepAliveChanged(int value)
        {
            _logger.LogInfo("Keep-alive interval changed to {0}", value);

            RestartIdleTimer();
        }

        private void RestartIdleTimer()
        {
            _logger.LogDebug("Resetting idle timeout to {0}", _attributes[AttributeNames.KeepAliveInterval]);

            _idleTimer.Change(TimeSpan.FromSeconds((int)_attributes[AttributeNames.KeepAliveInterval]), Timeout.InfiniteTimeSpan);
        }

        private void StopIdleTimer()
        {
            _idleTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private void RaiseMessageReceived(Payload payload)
        {
            MessageReceived?.Invoke(this, new PayloadEventArgs(payload));
        }
    }
}
