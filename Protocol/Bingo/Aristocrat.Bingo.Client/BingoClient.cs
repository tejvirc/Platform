namespace Aristocrat.Bingo.Client
{
    using Configuration;
    using Grpc.Core;
    using log4net;
    using Messages.Interceptor;
    using System.Reflection;
    using BingoClientApi = ServerApiGateway.ClientApi.ClientApiClient;

    public class BingoClient : BaseClient<BingoClientApi>, IClientEndpointProvider<BingoClientApi>
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

        public override BingoClientApi CreateClient(CallInvoker callInvoker)
        {
            return new (callInvoker);
        }
    }
}