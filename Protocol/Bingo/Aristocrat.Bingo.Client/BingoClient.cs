namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Configuration;
    using Extensions;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using log4net;
    using Messages.Interceptor;
    using ClientApi = ServerApiGateway.ClientApi.ClientApiClient;

    public class BingoClient : IClient, IClientEndpointProvider<ClientApi>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly TimeSpan StateChangeTimeOut = TimeSpan.FromSeconds(3);

        private readonly IClientConfigurationProvider _configurationProvider;
        private readonly BingoClientInterceptor _communicationInterceptor;
        private readonly object _clientLock = new();

        private Channel _channel;
        private bool _disposed;
        private ClientApi _client;
        private RpcConnectionState _state;

        public BingoClient(
            IClientConfigurationProvider configurationProvider,
            BingoClientInterceptor communicationInterceptor)
        {
            _configurationProvider =
                configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _communicationInterceptor = communicationInterceptor ??
                                        throw new ArgumentNullException(nameof(communicationInterceptor));

            _communicationInterceptor.MessageReceived += OnMessageReceived;
            _communicationInterceptor.AuthorizationFailed += OnAuthorizationFailed;
        }

        public event EventHandler<ConnectedEventArgs> Connected;

        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public event EventHandler<EventArgs> MessageReceived;

        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        public ClientApi Client
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

        public async Task<bool> Start()
        {
            try
            {
                await Stop().ConfigureAwait(false);
                using var configuration = _configurationProvider.CreateConfiguration();
                var credentials = configuration.Certificates.Any()
                    ? new SslCredentials(
                        string.Join(Environment.NewLine, configuration.Certificates.Select(x => x.ConvertToPem())))
                    : ChannelCredentials.Insecure;
                _channel = new Channel(configuration.Address.Host, configuration.Address.Port, credentials);
                var callInvoker = _channel.Intercept(_communicationInterceptor);
                if (configuration.ConnectionTimeout > TimeSpan.Zero)
                {
                    await _channel.ConnectAsync(DateTime.UtcNow + configuration.ConnectionTimeout)
                        .ConfigureAwait(false);
                }

                Client = new ClientApi(callInvoker);
                MonitorConnection();
                Connected?.Invoke(this, new ConnectedEventArgs());
            }
            catch (RpcException rpcException)
            {
                Logger.Error("Failed to connect the bingo client", rpcException);
                return false;
            }
            catch (OperationCanceledException operationCanceled)
            {
                Logger.Error("Failed to connect the bingo client", operationCanceled);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to connect to the bingo client", ex);
                return false;
            }

            return true;
        }

        public async Task<bool> Stop()
        {
            try
            {
                var channel = _channel;
                _channel = null;
                Client = null;

                if (channel != null)
                {
                    await channel.ShutdownAsync().ConfigureAwait(false);
                    Disconnected?.Invoke(this, new DisconnectedEventArgs());
                }
            }
            catch (RpcException rpcException)
            {
                Logger.Error("Failed to shutdown the bingo client", rpcException);
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _communicationInterceptor.MessageReceived -= OnMessageReceived;
                Stop().RunAndForget();
            }

            _disposed = true;
        }

        private static bool StateIsConnected(ChannelState? state) =>
            state is ChannelState.Ready or
                ChannelState.Idle or
                ChannelState.TransientFailure or
                ChannelState.Connecting;

        private static RpcConnectionState GetConnectionState(ChannelState state) =>
            state switch
            {
                ChannelState.Idle => RpcConnectionState.Disconnected,
                ChannelState.Connecting => RpcConnectionState.Connecting,
                ChannelState.Ready => RpcConnectionState.Connected,
                ChannelState.TransientFailure => RpcConnectionState.Disconnected,
                ChannelState.Shutdown => RpcConnectionState.Closed,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };

        private static async Task<ChannelState> WaitForStateChanges(Channel channel, ChannelState lastObservedState)
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

        private void OnMessageReceived(object sender, EventArgs e)
        {
            MessageReceived?.Invoke(this, EventArgs.Empty);
        }

        private void OnAuthorizationFailed(object sender, EventArgs e)
        {
            Stop().RunAndForget();
        }

        private void MonitorConnection()
        {
            Task.Run(async () => await MonitorConnectionAsync(_channel).ConfigureAwait(false)).RunAndForget(
                async e =>
                {
                    Logger.Error("Monitor Connection Failed Forcing a disconnect", e);
                    await Stop().ConfigureAwait(false);
                });
        }

        private async Task MonitorConnectionAsync(Channel channel)
        {
            if (channel is null)
            {
                return;
            }

            var lastObservedState = channel.State;
            Logger.Info($"Channel connection state changed: {lastObservedState}");
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
                Logger.Info($"Channel connection state changed: {lastObservedState}");
            }

            Logger.Error($"Channel connection is no longer connected: {lastObservedState}");
            await Stop().ConfigureAwait(false);
        }

        private void UpdateState(RpcConnectionState state)
        {
            if (state == _state || _disposed)
            {
                return;
            }

            _state = state;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(state));
        }
    }
}