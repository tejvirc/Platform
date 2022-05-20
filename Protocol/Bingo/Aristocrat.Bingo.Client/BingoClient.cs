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
    using ClientApi = ServerApiGateway.ClientApi.ClientApiClient;

    public class BingoClient : IClient, IClientEndpointProvider<ClientApi>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IClientConfigurationProvider _configurationProvider;
        private readonly BingoClientInterceptor _communicationInterceptor;
        private readonly object _clientLock = new();

        private Channel _channel;
        private bool _disposed;
        private ClientApi _client;

        public BingoClient(
            IClientConfigurationProvider configurationProvider,
            BingoClientInterceptor communicationInterceptor)
        {
            _configurationProvider =
                configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _communicationInterceptor = communicationInterceptor ??
                                        throw new ArgumentNullException(nameof(communicationInterceptor));

            _communicationInterceptor.MessageReceived += OnMessageReceived;
        }

        public event EventHandler<ConnectedEventArgs> Connected;

        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public event EventHandler<EventArgs> MessageReceived;

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

        public bool IsConnected => _channel?.State is ChannelState.Ready or ChannelState.Idle;

        public ClientConfigurationOptions Configuration => _configurationProvider.Configuration;

        public async Task<bool> Start()
        {
            try
            {
                await Stop();
                var configuration = _configurationProvider.Configuration;
                var credentials = configuration.Certificates.Any()
                    ? new SslCredentials(
                        string.Join(Environment.NewLine, configuration.Certificates.Select(x => x.ConvertToPem())))
                    : ChannelCredentials.Insecure;
                _channel = new Channel(configuration.Address.Host, configuration.Address.Port, credentials);
                var callInvoker = _channel.Intercept(_communicationInterceptor);
                if (configuration.ConnectionTimeout > TimeSpan.Zero)
                {
                    await _channel.ConnectAsync(DateTime.UtcNow + configuration.ConnectionTimeout);
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
                    await channel.ShutdownAsync();
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

        public void OnMessageReceived(object sender, EventArgs e)
        {
            MessageReceived?.Invoke(this, EventArgs.Empty);
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
                Stop().ContinueWith(
                    _ => Logger.Error("Stopping client failed while disposing"),
                    TaskContinuationOptions.OnlyOnFaulted);
            }

            _disposed = true;
        }

        private void MonitorConnection()
        {
            Task.Run(async () => await MonitorConnectionAsync()).ContinueWith(
                async _ =>
                {
                    Logger.Error("Monitor Connection Failed Forcing a disconnect");
                    await Stop();
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task MonitorConnectionAsync()
        {
            if (_channel is null)
            {
                return;
            }

            await _channel.WaitForStateChangedAsync(ChannelState.Ready);
            await Stop();
        }
    }
}