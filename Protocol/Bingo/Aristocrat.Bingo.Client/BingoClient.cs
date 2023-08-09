namespace Aristocrat.Bingo.Client
{
    using Configuration;
    using Grpc.Core;
    using Messages.Interceptor;
    using BingoClientApi = ServerApiGateway.ClientApi.ClientApiClient;

    public class BingoClient : BaseClient<BingoClientApi>, IClientEndpointProvider<BingoClientApi>
    {
        public BingoClient(
            IClientConfigurationProvider configurationProvider,
            BingoClientAuthorizationInterceptor authorizationInterceptor,
            LoggingInterceptor loggingInterceptor)
            : base(configurationProvider, authorizationInterceptor, loggingInterceptor)
        {
        }

        public override string FirewallRuleName => "Platform.Bingo.Server";

        public override BingoClientApi CreateClient(CallInvoker callInvoker)
        {
            return new (callInvoker);
        }
    }
}