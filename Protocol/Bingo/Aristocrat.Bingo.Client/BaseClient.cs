namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Configuration;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using Grpc.Net.Client;
    using log4net;
    using Messages.Interceptor;

    public abstract class BaseClient<TClientApi> : IClient
    {
        private readonly ILog _logger;
        private readonly BaseClientAuthorizationInterceptor _clientAuthorizationInterceptor;
        private readonly LoggingInterceptor _loggingInterceptor;

        private readonly object _clientLock = new();
        private GrpcChannel _channel;
        private TClientApi _client;
        private RpcConnectionState _state;
        private bool _disposed;

        protected BaseClient(
            IClientConfigurationProvider configurationProvider,
            BaseClientAuthorizationInterceptor authorizationInterceptor,
            LoggingInterceptor loggingInterceptor,
            ILog logger)
        {
            ConfigurationProvider =
                configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _clientAuthorizationInterceptor = authorizationInterceptor ?? throw new ArgumentNullException(nameof(authorizationInterceptor));
            _loggingInterceptor = loggingInterceptor ?? throw new ArgumentNullException(nameof(loggingInterceptor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggingInterceptor.MessageReceived += OnMessageReceived;
        }

        public abstract string FirewallRuleName { get; }

        public event EventHandler<ConnectedEventArgs> Connected;

        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public event EventHandler<EventArgs> MessageReceived;

		public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        public TClientApi Client
        {
            get
            {
                lock (_clientLock)
                {
                    return _client;
                }
            }
            set
            {
                lock (_clientLock)
                {
                    _client = value;
                }
            }
        }

		public bool IsConnected => StateIsConnected(_channel?.State);
		
        public virtual GrpcChannel CreateChannel()
        {
            var configuration = ConfigurationProvider.CreateConfiguration();
            var credentials = configuration.Certificates.Any()
                ? new SslCredentials(
                    string.Join(Environment.NewLine, configuration.Certificates.Select(x => x.ConvertToPem())))
                : ChannelCredentials.Insecure;
            return GrpcChannel.ForAddress(configuration.Address, new GrpcChannelOptions());
        }

        public abstract GrpcChannel CreateChannel();

        public abstract TClientApi CreateClient(CallInvoker callInvoker);

        public async Task<bool> Start()
        {
            try
            {
                await Stop().ConfigureAwait(false);
                _channel = CreateChannel();
                var callInvoker = _channel.Intercept(_clientAuthorizationInterceptor, _loggingInterceptor);
                var configuration = ConfigurationProvider.CreateConfiguration();
                if (configuration.ConnectionTimeout > TimeSpan.Zero)
                {
                    using var source = new CancellationTokenSource(configuration.ConnectionTimeout);
                    await _channel.ConnectAsync(source.Token)
                        .ConfigureAwait(false);
                }

                Client = CreateClient(callInvoker);
                MonitorConnection();
                Connected?.Invoke(this, new ConnectedEventArgs());
                return true;
            }
            catch (RpcException rpcException)
            {
                _logger.Error($"Failed to connect the {GetType().Name}", rpcException);
            }
            catch (OperationCanceledException operationCanceled)
            {
                _logger.Error($"Failed to connect the {GetType().Name}", operationCanceled);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to connect to the {GetType().Name}", ex);
            }

            return false;
        }

        public async Task<bool> Stop()
        {
            try
            {
                var channel = _channel;
                _channel = null;
                Client = default;

                if (channel != null)
                {
                    await channel.ShutdownAsync().ConfigureAwait(false);
                    Disconnected?.Invoke(this, new DisconnectedEventArgs());
                }
            }
            catch (RpcException rpcException)
            {
                _logger.Error($"Failed to shutdown the {GetType().Name}", rpcException);
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void OnMessageReceived(object sender, EventArgs e)
        {
            MessageReceived?.Invoke(this, EventArgs.Empty);
        }

        protected static RpcConnectionState GetConnectionState(ConnectivityState state) =>
            state switch
            {
                ConnectivityState.Idle => RpcConnectionState.Disconnected,
                ConnectivityState.Connecting => RpcConnectionState.Connecting,
                ConnectivityState.Ready => RpcConnectionState.Connected,
                ConnectivityState.TransientFailure => RpcConnectionState.Disconnected,
                ConnectivityState.Shutdown => RpcConnectionState.Closed,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };

        protected static bool StateIsConnected(ConnectivityState? state) =>
            state is ConnectivityState.Ready or
                ConnectivityState.Idle or
                ConnectivityState.TransientFailure or
                ConnectivityState.Connecting;

        protected static async Task<ConnectivityState> WaitForStateChanges(GrpcChannel channel, ConnectivityState lastObservedState)
        {
            if (lastObservedState is ConnectivityState.Ready)
            {
                await channel.WaitForStateChangedAsync(lastObservedState).ConfigureAwait(false);
            }
            else
            {
                using var source = new CancellationTokenSource(ClientConstants.StateChangeTimeOut);
                await channel.WaitForStateChangedAsync(lastObservedState, source.Token).ConfigureAwait(false);
            }

            return channel.State;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _loggingInterceptor.MessageReceived -= OnMessageReceived;
                Stop().ContinueWith(
                    _ => _logger.Error($"Stopping {GetType().Name} failed while disposing"),
                    TaskContinuationOptions.OnlyOnFaulted);
                _channel?.Dispose();
                _channel = null;
            }

            _disposed = true;
        }

        protected virtual async Task MonitorConnectionAsync(GrpcChannel channel)
        {
            if (channel is null)
            {
                return;
            }

            var lastObservedState = channel.State;
            _logger.Info($"{GetType().Name} Channel last observed state: {lastObservedState}");
            UpdateState(GetConnectionState(lastObservedState));
            while (StateIsConnected(lastObservedState))
            {
                var observedState = await WaitForStateChanges(channel, lastObservedState).ConfigureAwait(false);
                if (lastObservedState is not ConnectivityState.Ready && observedState == lastObservedState)
                {
                    break;
                }

                lastObservedState = observedState;
                UpdateState(GetConnectionState(lastObservedState));
                _logger.Info($"{GetType().Name} Channel connection state changed: {lastObservedState}");
            }

            _logger.Error($"{GetType().Name} Channel connection is no longer connected: {lastObservedState}");
            await Stop().ConfigureAwait(false);
        }

        protected void UpdateState(RpcConnectionState state)
        {
            if (state == _state || _disposed)
            {
                return;
            }

            _state = state;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(state));
        }

        private void MonitorConnection()
        {
            Task.Run(async () => await MonitorConnectionAsync(_channel)).ContinueWith(
                async _ =>
                {
                    _logger.Error($"Monitor Connection Failed for {GetType().Name} failed Forcing a disconnect");
                    await Stop();
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}