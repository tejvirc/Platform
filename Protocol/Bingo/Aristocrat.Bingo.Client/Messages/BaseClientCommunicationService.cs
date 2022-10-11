namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Polly;
    using ServerApiGateway;

    public abstract class BaseClientCommunicationService
    {
        private const int RetryCount = 3;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(100);

        private readonly IClientEndpointProvider<ClientApi.ClientApiClient> _endpointProvider;

        protected BaseClientCommunicationService(IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
        {
            _endpointProvider = endpointProvider ?? throw new ArgumentNullException(nameof(endpointProvider));
        }

        protected Task<TResult> Invoke<TResult>(
            Func<ClientApi.ClientApiClient, Task<TResult>> action,
            IAsyncPolicy policy = null) =>
            Invoke(async (c, _) => await action(c).ConfigureAwait(false), CancellationToken.None, policy);

        protected async Task<TResult> Invoke<TResult>(
            Func<ClientApi.ClientApiClient, CancellationToken, Task<TResult>> action,
            CancellationToken token,
            IAsyncPolicy policy = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var client = GetClient();
            if (policy is null)
            {
                return await action(client, token).ConfigureAwait(false);
            }

            return await policy.ExecuteAsync(async t => await action(client, t).ConfigureAwait(false), token)
                .ConfigureAwait(false);
        }

        protected static IAsyncPolicy CreatePolicy(int retryCount = RetryCount, Func<int, TimeSpan> delay = null)
        {
            return Policy
                .Handle<RpcException>(e => e.StatusCode is not (StatusCode.Cancelled or StatusCode.Aborted))
                .OrInner<RpcException>(e => e.StatusCode is not (StatusCode.Cancelled or StatusCode.Aborted))
                .WaitAndRetryAsync(retryCount, delay ?? (_ => RetryDelay));
        }

        protected AsyncDuplexStreamingCall<TInput, TOutput> Invoke<TInput, TOutput>(
            Func<ClientApi.ClientApiClient, AsyncDuplexStreamingCall<TInput, TOutput>> action) =>
            Invoke((c, _) => action(c), CancellationToken.None);

        protected AsyncDuplexStreamingCall<TInput, TOutput> Invoke<TInput, TOutput>(
            Func<ClientApi.ClientApiClient, CancellationToken, AsyncDuplexStreamingCall<TInput, TOutput>> action,
            CancellationToken token)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var client = GetClient();
            return action(client, token);
        }

        protected AsyncServerStreamingCall<TOutput> Invoke<TOutput>(
            Func<ClientApi.ClientApiClient, AsyncServerStreamingCall<TOutput>> action) =>
            Invoke((c, _) => action(c), CancellationToken.None);

        protected AsyncServerStreamingCall<TOutput> Invoke<TOutput>(
            Func<ClientApi.ClientApiClient, CancellationToken, AsyncServerStreamingCall<TOutput>> action,
            CancellationToken token)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var client = GetClient();
            return action(client, token);
        }

        private ClientApi.ClientApiClient GetClient()
        {
            var client = _endpointProvider.Client;
            if (!_endpointProvider.IsConnected || client is null)
            {
                throw new InvalidOperationException();
            }

            return client;
        }
    }
}