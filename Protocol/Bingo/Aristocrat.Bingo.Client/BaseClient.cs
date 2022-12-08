namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Configuration;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using log4net;
    using Messages.Interceptor;

    public abstract class BaseClient<TClientApi> : IClient
    {
        protected readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        protected readonly IClientConfigurationProvider ConfigurationProvider;
        protected readonly BaseClientInterceptor CommunicationInterceptor;

        private readonly object _clientLock = new();
        private Channel _channel;
        private TClientApi _client;
        private bool _disposed;

        protected BaseClient(
            IClientConfigurationProvider configurationProvider,
            BaseClientInterceptor communicationInterceptor)
        {
            ConfigurationProvider =
                configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            CommunicationInterceptor = communicationInterceptor ??
                                       throw new ArgumentNullException(nameof(communicationInterceptor));

            CommunicationInterceptor.MessageReceived += OnMessageReceived;
        }

        public abstract string FirewallRuleName { get; }

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

        public event EventHandler<ConnectedEventArgs> Connected;

        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public event EventHandler<EventArgs> MessageReceived;

        public bool IsConnected => _channel?.State is ChannelState.Ready or ChannelState.Idle;

        public ClientConfigurationOptions Configuration => ConfigurationProvider.Configuration;

        public abstract Channel CreateChannel();

        public abstract TClientApi CreateClient(CallInvoker callInvoker);

        public async Task<bool> Start()
        {
            try
            {
                await Stop();
                _channel = CreateChannel();
                var configuration = ConfigurationProvider.Configuration;
                var callInvoker = _channel.Intercept(CommunicationInterceptor);
                if (configuration.ConnectionTimeout > TimeSpan.Zero)
                {
                    await _channel.ConnectAsync(DateTime.UtcNow + configuration.ConnectionTimeout);
                }

                Client = CreateClient(callInvoker);
                MonitorConnection();
                Connected?.Invoke(this, new ConnectedEventArgs());
            }
            catch (RpcException rpcException)
            {
                Logger.Error($"Failed to connect the {GetType().Name}", rpcException);
                return false;
            }
            catch (OperationCanceledException operationCanceled)
            {
                Logger.Error($"Failed to connect the {GetType().Name}", operationCanceled);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to connect to the {GetType().Name}", ex);
                return false;
            }

            return true;
        }

        public virtual async Task<bool> Stop()
        {
            try
            {
                Client = default;
                var channel = _channel;
                _channel = null;

                if (channel != null)
                {
                    await channel.ShutdownAsync();
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
                CommunicationInterceptor.MessageReceived -= OnMessageReceived;
                Stop().ContinueWith(
                    _ => Logger.Error($"Stopping {GetType().Name} failed while disposing"),
                    TaskContinuationOptions.OnlyOnFaulted);
            }

            _disposed = true;
        }

        private void MonitorConnection()
        {
            Task.Run(async () => await MonitorConnectionAsync()).ContinueWith(
                async _ =>
                {
                    Logger.Error($"Monitor connection for {GetType().Name} failed forcing a disconnect");
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