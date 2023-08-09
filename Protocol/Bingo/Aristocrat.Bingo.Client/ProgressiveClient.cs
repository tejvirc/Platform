﻿namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Configuration;
    using Grpc.Core;
    using Messages.Interceptor;
    using ProgressiveClientApi = ServerApiGateway.ProgressiveApi.ProgressiveApiClient;

    public class ProgressiveClient : BaseClient<ProgressiveClientApi>, IClientEndpointProvider<ProgressiveClientApi>
    {
        public ProgressiveClient(
            IClientConfigurationProvider configurationProvider,
            ProgressiveClientAuthorizationInterceptor authorizationInterceptor,
            LoggingInterceptor loggingInterceptor)
            : base(configurationProvider, authorizationInterceptor, loggingInterceptor)
        {
        }

        public override string FirewallRuleName => "Platform.Bingo.ProgressiveServer";

        public override ProgressiveClientApi CreateClient(CallInvoker callInvoker)
        {
            return new (callInvoker);
        }

        protected override async Task MonitorConnectionAsync(Channel channel)
        {
            if (channel is null)
            {
                return;
            }

            var lastObservedState = channel.State;
            Logger.Info($"{GetType().Name} Channel last observed state: {lastObservedState}");
            UpdateState(GetConnectionState(lastObservedState));
            while (StateIsConnected(lastObservedState))
            {
                var observedState = await WaitForStateChanges(channel, lastObservedState).ConfigureAwait(false);

                // The progressive client does not send regular messages on the channel so it will go idle. Do not disconnect in this situation.
                if (lastObservedState is not ChannelState.Ready && lastObservedState is not ChannelState.Idle && observedState == lastObservedState)
                {
                    break;
                }

                lastObservedState = observedState;
                UpdateState(GetConnectionState(lastObservedState));
                Logger.Info($"{GetType().Name} Channel connection state changed: {lastObservedState}");
            }

            Logger.Error($"{GetType().Name} Channel connection is no longer connected: {lastObservedState}");
            await Stop().ConfigureAwait(false);
        }
    }
}