namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Linq;
    using Configuration;
    using Grpc.Core;
    using Messages.Interceptor;
    using ClientApi = ServerApiGateway.ProgressiveApi.ProgressiveApiClient;

    public class ProgressiveClient : BaseClient<ClientApi>, IClientEndpointProvider<ClientApi>
    {
        public ProgressiveClient(
            IClientConfigurationProvider configurationProvider,
            ProgressiveClientInterceptor communicationInterceptor)
            : base(configurationProvider, communicationInterceptor)
        {
        }

        public override string FirewallRuleName => "Platform.Bingo.ProgressiveServer";

        public override Channel CreateChannel()
        {
            var configuration = ConfigurationProvider.Configuration;
            var credentials = configuration.Certificates.Any()
                ? new SslCredentials(
                    string.Join(Environment.NewLine, configuration.Certificates.Select(x => x.ConvertToPem())))
                : ChannelCredentials.Insecure;
            // TODO: For now must use port 5085 for progressive communications. This will change when server is updated in the future.
            return new Channel(configuration.Address.Host, 5085, credentials);
        }

        public override ClientApi CreateClient(CallInvoker callInvoker)
        {
            return new (callInvoker);
        }
    }
}