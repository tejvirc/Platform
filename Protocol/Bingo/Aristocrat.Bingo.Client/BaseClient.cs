namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Configuration;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using log4net;
    using Messages.Interceptor;

    public abstract class BaseClient<TClientApi> : IClient
    {
        protected readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
		protected static readonly TimeSpan StateChangeTimeOut = TimeSpan.FromSeconds(3);
        protected readonly IClientConfigurationProvider ConfigurationProvider;
        protected readonly BaseClientAuthorizationInterceptor ClientAuthorizationInterceptor;
        protected readonly LoggingInterceptor LoggingInterceptor;

        private readonly object _clientLock = new();
        private Channel _channel;
        private TClientApi _client;
        private RpcConnectionState _state;
        private bool _disposed;

        protected BaseClient(
            IClientConfigurationProvider configurationProvider,
            BaseClientAuthorizationInterceptor authorizationInterceptor,
            LoggingInterceptor loggingInterceptor)
        {
            ConfigurationProvider =
                configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            ClientAuthorizationInterceptor = authorizationInterceptor ?? throw new ArgumentNullException(nameof(authorizationInterceptor));
            LoggingInterceptor = loggingInterceptor ?? throw new ArgumentNullException(nameof(loggingInterceptor));

            LoggingInterceptor.MessageReceived += OnMessageReceived;
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
		
        public virtual Channel CreateChannel()
        {
            var configuration = ConfigurationProvider.CreateConfiguration();
            var credentials = configuration.Certificates.Any()
                ? new SslCredentials(
                    string.Join(Environment.NewLine, configuration.Certificates.Select(x => x.ConvertToPem())))
                : ChannelCredentials.Insecure;
            return new Channel(configuration.Address.Host, configuration.Address.Port, credentials);
        }

        public abstract TClientApi CreateClient(CallInvoker callInvoker);

        public async Task<bool> Start()
        {
            try
            {
                await Stop().ConfigureAwait(false);
                _channel = CreateChannel();
                var callInvoker = _channel.Intercept(ClientAuthorizationInterceptor, LoggingInterceptor);
                var configuration = ConfigurationProvider.CreateConfiguration();
                if (configuration.ConnectionTimeout > TimeSpan.Zero)
                {
                    await _channel.ConnectAsync(DateTime.UtcNow + configuration.ConnectionTimeout)
                        .ConfigureAwait(false);
                }

                Client = CreateClient(callInvoker);
                MonitorConnection();
                Connected?.Invoke(this, new ConnectedEventArgs());
                return true;
            }
            catch (RpcException rpcException)
            {
                Logger.Error($"Failed to connect the {GetType().Name}", rpcException);
            }
            catch (OperationCanceledException operationCanceled)
            {
                Logger.Error($"Failed to connect the {GetType().Name}", operationCanceled);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to connect to the {GetType().Name}", ex);
            }

            return false;
        }

        public virtual async Task<bool> Stop()
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
                Logger.Error($"Failed to shutdown the {GetType().Name}", rpcException);
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

        protected static RpcConnectionState GetConnectionState(ChannelState state) =>
            state switch
            {
                ChannelState.Idle => RpcConnectionState.Disconnected,
                ChannelState.Connecting => RpcConnectionState.Connecting,
                ChannelState.Ready => RpcConnectionState.Connected,
                ChannelState.TransientFailure => RpcConnectionState.Disconnected,
                ChannelState.Shutdown => RpcConnectionState.Closed,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };

        protected static bool StateIsConnected(ChannelState? state) =>
            state is ChannelState.Ready or
                ChannelState.Idle or
                ChannelState.TransientFailure or
                ChannelState.Connecting;

        protected static async Task<ChannelState> WaitForStateChanges(Channel channel, ChannelState lastObservedState)
        {
            if (lastObservedState is ChannelState.Ready)
            {
                await channel.TryWaitForStateChangedAsync(lastObservedState).ConfigureAwait(false);
            }
            else
            {
                await channel.TryWaitForStateChangedAsync(
                    lastObservedState,
                    DateTime.UtcNow.Add(StateChangeTimeOut)).ConfigureAwait(false);
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
                LoggingInterceptor.MessageReceived -= OnMessageReceived;
                Stop().ContinueWith(
                    _ => Logger.Error($"Stopping {GetType().Name} failed while disposing"),
                    TaskContinuationOptions.OnlyOnFaulted);
            }

            _disposed = true;
        }

        protected virtual async Task MonitorConnectionAsync(Channel channel)
        {
            if (channel is null)
            {
                return;
            }

            var lastObservedState = channel.State;
            Logger.Info($"{GetType().Name} Channel last observed state: {lastObservedState}");
            UpdateState(GetConnectionState(lastObservedState));
            while (StateIsConnected(lastObservedState))
            {
                var observedState = await WaitForStateChanges(channel, lastObservedState).ConfigureAwait(false);
                if (lastObservedState is not ChannelState.Ready && observedState == lastObservedState)
                {
                    break;
                }

                lastObservedState = observedState;
                UpdateState(GetConnectionState(lastObservedState));
                Logger.Info($"{GetType().Name} Channel connection state changed: {lastObservedState}");
            }

            Logger.Error($"{GetType().Name} Channel connection is no longer connected: {lastObservedState}");
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
                    Logger.Error($"Monitor Connection Failed for {GetType().Name} failed Forcing a disconnect");
                    await Stop();
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}