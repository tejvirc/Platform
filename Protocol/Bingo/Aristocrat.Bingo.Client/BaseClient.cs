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

    public abstract class BaseClient : IClient
    {
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        protected readonly IClientConfigurationProvider ConfigurationProvider;
        protected readonly BingoClientInterceptor CommunicationInterceptor;

        protected Channel Channel;
        private bool _disposed;

        protected BaseClient(
            IClientConfigurationProvider configurationProvider,
            BingoClientInterceptor communicationInterceptor)
        {
            ConfigurationProvider =
                configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            CommunicationInterceptor = communicationInterceptor ??
                                       throw new ArgumentNullException(nameof(communicationInterceptor));

            CommunicationInterceptor.MessageReceived += OnMessageReceived;
        }

        public event EventHandler<ConnectedEventArgs> Connected;

        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public event EventHandler<EventArgs> MessageReceived;

        public bool IsConnected => Channel?.State is ChannelState.Ready or ChannelState.Idle;

        public ClientConfigurationOptions Configuration => ConfigurationProvider.Configuration;

        public abstract void CreateChannel();

        public abstract void CreateClient(CallInvoker callInvoker);

        public async Task<bool> Start()
        {
            try
            {
                await Stop();
                CreateChannel();
                var configuration = ConfigurationProvider.Configuration;
                var callInvoker = Channel.Intercept(CommunicationInterceptor);
                if (configuration.ConnectionTimeout > TimeSpan.Zero)
                {
                    await Channel.ConnectAsync(DateTime.UtcNow + configuration.ConnectionTimeout);
                }

                CreateClient(callInvoker);
                MonitorConnection();
                Connected?.Invoke(this, new ConnectedEventArgs());
            }
            catch (RpcException rpcException)
            {
                Logger.Error("Failed to connect the progressive client", rpcException);
                return false;
            }
            catch (OperationCanceledException operationCanceled)
            {
                Logger.Error("Failed to connect the progressive client", operationCanceled);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to connect to the progressive client", ex);
                return false;
            }

            return true;
        }

        public virtual async Task<bool> Stop()
        {
            try
            {
                var channel = Channel;
                Channel = null;

                if (channel != null)
                {
                    await channel.ShutdownAsync();
                    Disconnected?.Invoke(this, new DisconnectedEventArgs());
                }
            }
            catch (RpcException rpcException)
            {
                Logger.Error("Failed to shutdown the progressive client", rpcException);
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
            if (Channel is null)
            {
                return;
            }

            await Channel.WaitForStateChangedAsync(ChannelState.Ready);
            await Stop();
        }
    }
}