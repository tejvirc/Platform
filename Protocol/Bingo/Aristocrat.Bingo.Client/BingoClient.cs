namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Configuration;
    using Grpc.Core;
    using Messages.Interceptor;
    using ClientApi = ServerApiGateway.ClientApi.ClientApiClient;

    public class BingoClient : BaseClient<ClientApi>, IClientEndpointProvider<ClientApi>
    {
        public BingoClient(
            IClientConfigurationProvider configurationProvider,
            BingoClientInterceptor communicationInterceptor)
            : base(configurationProvider, communicationInterceptor)
        {
        }

        public override string FirewallRuleName => "Platform.Bingo.Server";

        public override Channel CreateChannel()
        {
            var configuration = ConfigurationProvider.Configuration;
            var credentials = configuration.Certificates.Any()
                ? new SslCredentials(
                    string.Join(Environment.NewLine, configuration.Certificates.Select(x => x.ConvertToPem())))
                : ChannelCredentials.Insecure;
            return new Channel(configuration.Address.Host, configuration.Address.Port, credentials);
        }

        public override ClientApi CreateClient(CallInvoker callInvoker)
        {
            return new (callInvoker);
        }

        public override async Task<bool> Stop()
        {
            Client = null;
            return await base.Stop();
        }
    }
}