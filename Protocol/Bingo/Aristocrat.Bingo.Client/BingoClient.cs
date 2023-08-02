namespace Aristocrat.Bingo.Client
{
    using System.Reflection;
    using Configuration;
    using Grpc.Core;
    using Grpc.Net.Client;
    using log4net;
    using Messages.Interceptor;
    using ClientApi = ServerApiGateway.ClientApi.ClientApiClient;

    public class BingoClient : BaseClient<ClientApi>, IClientEndpointProvider<ClientApi>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public BingoClient(
            IClientConfigurationProvider configurationProvider,
            BingoClientAuthorizationInterceptor authorizationInterceptor,
            LoggingInterceptor loggingInterceptor)
            : base(
                configurationProvider,
                authorizationInterceptor,
                loggingInterceptor,
                Logger)
        {
        }

        public override string FirewallRuleName => "Platform.Bingo.Server";

        public override GrpcChannel CreateChannel()
        {
            var configuration = ConfigurationProvider.CreateConfiguration();
            return GrpcChannel.ForAddress(configuration.Address, new GrpcChannelOptions());
        }

        public override ClientApi CreateClient(CallInvoker callInvoker)
        {
            return new (callInvoker);
        }
    }
}