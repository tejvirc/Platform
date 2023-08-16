namespace Aristocrat.Bingo.Client
{
    using System.Reflection;
    using Configuration;
    using Grpc.Core;
    using Grpc.Net.Client;
    using log4net;
    using Messages.Interceptor;
    using BingoClientApi = ServerApiGateway.ClientApi.ClientApiClient;

    public class BingoClient : BaseClient<BingoClientApi>, IClientEndpointProvider<BingoClientApi>
    {

        public BingoClient(
            IClientConfigurationProvider configurationProvider,
            BingoClientAuthorizationInterceptor authorizationInterceptor,
            LoggingInterceptor loggingInterceptor,
            ILog logger)
            : base(
                configurationProvider,
                authorizationInterceptor,
                loggingInterceptor, logger)
        {
        }

        public override string FirewallRuleName => "Platform.Bingo.Server";

        public override BingoClientApi CreateClient(CallInvoker callInvoker)
        {
            return new (callInvoker);
        }
    }
}