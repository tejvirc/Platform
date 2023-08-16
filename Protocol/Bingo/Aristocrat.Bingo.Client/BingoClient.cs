﻿namespace Aristocrat.Bingo.Client
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