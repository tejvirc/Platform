namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Client;
    using Common;
    using Helpers;
    using Logging;
    using Messaging;
    using Monaco.Common;
    using Options;

    /// <summary>
    ///     Transports messages to the host over UDP sockets.
    /// </summary>
    internal class BroadcastTransporter : IBroadcastTransporter, IStartable, IDisposable
    {
        private const int ClientLockTimeout = 15;
        private const int ThrottleDelay = 1;
        private const int ThrottleTimeout = 2;

        private readonly ILogger<BroadcastTransporter> _logger;
        private readonly IOptionsMonitor<ProtocolOptions> _options;
        private readonly IClientFactory _clientFactory;
        private readonly ITransportPublisher _publisher;

        private ManualResetEvent _connected = new(false);

        private SubscriptionList _subscriptions = new();

        private SemaphoreSlim _clientLock = new(1);

        private readonly IObservable<EventPattern<PayloadEventArgs>> _messages;

        private IBroadcastClient _client;

        private IPEndPoint _directoryEndPoint;

        private ManualResetEvent _throttleEvent = new(true);

        private Timer _throttleTimer;

        private CancellationTokenSource _shutdown = new();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BroadcastTransporter"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="options"><see cref="IOptionsMonitor{TOptions}"/>.</param>
        /// <param name="clientFactory"><see cref="IClientFactory"/>.</param>
        /// <param name="publisher"><see cref="ITransportPublisher"/>.</param>
        public BroadcastTransporter(
            ILogger<BroadcastTransporter> logger,
            IOptionsMonitor<ProtocolOptions> options,
            IClientFactory clientFactory,
            ITransportPublisher publisher)
        {
            _logger = logger;
            _options = options;
            _clientFactory = clientFactory;
            _publisher = publisher;

            _messages = Observable.FromEventPattern<PayloadEventArgs>(
                h => MessageReceived += h,
                h => MessageReceived -= h);

            _subscriptions.Add(
                _options.OnChange(
                    OnOptionsChanged,
                    name => name == nameof(ProtocolOptions.DirectoryAddress) || name == nameof(ProtocolOptions.NetworkAddress)));

            _throttleTimer = new Timer(_ => OnThrottleTick(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        /// <inheritdoc />
        ~BroadcastTransporter()
        {
            Dispose(false);
        }

        private event EventHandler<PayloadEventArgs> MessageReceived;

        /// <inheritdoc />
        public IObservable<Payload?> Messages => _messages.Select(e => e.EventArgs?.Payload).AsObservable();

        /// <inheritdoc />
        public IPEndPoint EndPoint => _directoryEndPoint;

        /// <inheritdoc />
        public Task Broadcast(Payload payload, CancellationToken cancellationToken)
        {
            if (_shutdown.IsCancellationRequested)
            {
                throw new InvalidOperationException("Shutdown requested");
            }

            InvokeClient(
                client =>
                {
                    try
                    {
                        if (!_throttleEvent.WaitOne(TimeSpan.FromSeconds(ThrottleTimeout)))
                        {
                            return;
                        }

                        _logger.LogDebug(
                            "Sending message of size {0} to {1}, Thread: {2}",
                            payload.MessageSize,
                            _directoryEndPoint,
                            Thread.CurrentThread.ManagedThreadId);

                        client.Send(_directoryEndPoint, payload.Content);
                    }
                    finally
                    {
                        Throttle();
                    }
                });

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task Start()
        {
            _logger.LogInfo($"Starting {GetType().Name}...");

            _directoryEndPoint = _options.CurrentValue.DirectoryAddress;

            Connect();

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
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing"></param>
        [SuppressMessage("ReSharper", "UseNullPropagation")]
        protected virtual void Dispose(bool disposing)
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

                if (_throttleEvent != null)
                {
                    _throttleEvent.Dispose();
                }

                if (_throttleTimer != null)
                {
                    _throttleTimer.Dispose();
                }

                if (_clientLock != null)
                {
                    _clientLock.Dispose();
                }

                if (_subscriptions != null)
                {
                    _subscriptions.Dispose();
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

            _throttleEvent = null;
            _throttleTimer = null;
            _client = null;
            _clientLock = null;
            _subscriptions = null;
            _connected = null;
            _shutdown = null;

            _disposed = true;
        }

        private void Connect()
        {
            if (_shutdown.IsCancellationRequested)
            {
                throw new InvalidOperationException("Shutdown requested");
            }

            if (!_clientLock.Wait(TimeSpan.FromSeconds(ClientLockTimeout)))
            {
                throw new InvalidOperationException("Client is locked");
            }

            try
            {
                if (_client is { IsConnected: true })
                {
                    throw new InvalidOperationException("Already connected");
                }

                var responsePort = NetworkHelper.GetNextUdpPort();

                _options.CurrentValue.DirectoryResponseAddress = new IPEndPoint(
                    _options.CurrentValue.NetworkAddress,
                    responsePort);

                _client = _clientFactory.CreateBroadcastClient(new IPEndPoint(IPAddress.Any, responsePort));

                _client.IsMulticast = true;

                _client.Connected += OnConnected;
                _client.Disconnected += OnDisconnected;
                _client.Received += OnReceived;
                _client.Error += OnError;

                if (!_client.IsConnected)
                {
                    _client.Connect();
                    _client.EnableBroadcast = true;
                }
            }
            finally
            {
                _clientLock.Release();
            }
        }

        private void Disconnect()
        {
            if (!_clientLock.Wait(TimeSpan.FromSeconds(ClientLockTimeout)))
            {
                throw new InvalidOperationException("Client is locked");
            }

            try
            {
                if (_client == null)
                {
                    return;
                }

                if (_client.IsConnected)
                {
                    _client.Disconnect();
                }
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

        private void InvokeClient(Action<IBroadcastClient> action)
        {
            var client = _client;
            if (client == null || !_connected.WaitOne(TimeSpan.Zero))
            {
                throw new InvalidOperationException("No Connection");
            }

            action.Invoke(client);
        }

        private void Throttle()
        {
            _throttleEvent.Reset();

            _throttleTimer = new Timer(_ => OnThrottleTick(), null, TimeSpan.FromSeconds(ThrottleDelay), Timeout.InfiniteTimeSpan);
        }

        private void OnThrottleTick()
        {
            _throttleEvent.Set();
        }

        private void OnConnected(object sender, ConnectedEventArgs args)
        {
            _logger.LogInfo("UDP client connected at {0}", _directoryEndPoint);

            _connected.Set();

            _publisher.Publish(new TransportStatus
            (
                TransportState.Up,
                true,
                EndPoint,
                ConnectionState.Connected
            ));
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs args)
        {
            _logger.LogWarn("UDP client disconnected at {0}", _directoryEndPoint);

            _connected.Reset();

            _publisher.Publish(new TransportStatus
            (
                TransportState.Down,
                true,
                EndPoint,
                ConnectionState.Lost
            ));

            Task.Run(Connect).FireAndForget(ex => _logger.LogError(ex, ex.Message));
        }

        private void OnReceived(object sender, ReceivedEventArgs args)
        {
            _logger.LogDebug(
                "UDP client received message at {0} of size {1}, Thread: {2}",
                args.EndPoint,
                args.Size,
                Thread.CurrentThread.ManagedThreadId);

            try
            {
                var payload = MessageHelper.ParseResponse(args.Buffer);

                payload.IsBroadcast = true;
                payload.EndPoint = args.EndPoint;

                RaiseMessageReceived(payload);
            }
            catch (Exception ex)
            {
                _publisher.Publish(
                    new TransportStatus(
                        TransportState.Up,
                        true,
                        EndPoint,
                        ConnectionState.Unchanged,
                        TransportFailure.Malformed));

                _logger.LogError(ex, "UDP client received malformed message at {0}", args.EndPoint);
            }
        }

        private void OnError(object sender, ErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "TCP client error at {0}", _directoryEndPoint);
        }

        private void OnOptionsChanged(ProtocolOptions options, string propertyName)
        {
            switch (propertyName)
            {
                case nameof(ProtocolOptions.DirectoryAddress):
                    _directoryEndPoint = options.DirectoryAddress;
                    Disconnect();
                    break;

                case nameof(ProtocolOptions.NetworkAddress):
                    Disconnect();
                    break;
            }
        }

        private void RaiseMessageReceived(Payload payload)
        {
            MessageReceived?.Invoke(this, new PayloadEventArgs(payload));
        }
    }
}
