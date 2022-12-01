namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Configuration;
    using Grpc.Core;
    using Messages.Interceptor;
    using ClientApi = ServerApiGateway.ProgressiveApi.ProgressiveApiClient;

    public class ProgressiveClient : BaseClient, IClientEndpointProvider<ClientApi>
    {
        private readonly object _clientLock = new();
        private ClientApi _client;

        public ProgressiveClient(
            IClientConfigurationProvider configurationProvider,
            BingoClientInterceptor communicationInterceptor)
            : base(configurationProvider, communicationInterceptor)
        {
        }

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

        public override void CreateChannel()
        {
            var configuration = ConfigurationProvider.Configuration;
            var credentials = configuration.Certificates.Any()
                ? new SslCredentials(
                    string.Join(Environment.NewLine, configuration.Certificates.Select(x => x.ConvertToPem())))
                : ChannelCredentials.Insecure;
            // TODO: For now must use port 5085 for progressive communications. This will change when server is updated in the future.
            Channel = new Channel(configuration.Address.Host, 5085, credentials);
        }

        public override void CreateClient(CallInvoker callInvoker)
        {
            Client = new ClientApi(callInvoker);
        }

        public override async Task<bool> Stop()
        {
            Client = null;
            var result = await base.Stop();
            return result;
        }
    }
}