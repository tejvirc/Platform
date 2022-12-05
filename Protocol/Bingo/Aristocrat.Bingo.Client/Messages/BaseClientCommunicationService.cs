namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Polly;
    public abstract class BaseClientCommunicationService<T> where T : ClientBase<T>
    {
        private const int RetryCount = 3;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(100);
        private readonly IClientEndpointProvider<T> _endpointProvider;
        protected BaseClientCommunicationService(IClientEndpointProvider<T> endpointProvider)
        {
            _endpointProvider = endpointProvider ?? throw new ArgumentNullException(nameof(endpointProvider));
        }

        protected static IAsyncPolicy CreatePolicy(int retryCount = RetryCount, Func<int, TimeSpan> delay = null)
        {
            return Policy
                .Handle<RpcException>(e => e.StatusCode is not (StatusCode.Cancelled or StatusCode.Aborted))
                .OrInner<RpcException>(e => e.StatusCode is not (StatusCode.Cancelled or StatusCode.Aborted))
                .WaitAndRetryAsync(retryCount, delay ?? (_ => RetryDelay));
        }

        protected Task<TResult> Invoke<TResult>(
            Func<T, Task<TResult>> action,
            IAsyncPolicy policy = null) =>
            Invoke(async (c, _) => await action(c).ConfigureAwait(false), CancellationToken.None, policy);
        protected async Task<TResult> Invoke<TResult>(
            Func<T, CancellationToken, Task<TResult>> action,
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

        protected AsyncDuplexStreamingCall<TInput, TOutput> Invoke<TInput, TOutput>(
            Func<T, AsyncDuplexStreamingCall<TInput, TOutput>> action) =>
            Invoke((c, _) => action(c), CancellationToken.None);
        protected AsyncDuplexStreamingCall<TInput, TOutput> Invoke<TInput, TOutput>(
            Func<T, CancellationToken, AsyncDuplexStreamingCall<TInput, TOutput>> action,
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
            Func<T, AsyncServerStreamingCall<TOutput>> action) =>
            Invoke((c, _) => action(c), CancellationToken.None);
        protected AsyncServerStreamingCall<TOutput> Invoke<TOutput>(
            Func<T, CancellationToken, AsyncServerStreamingCall<TOutput>> action,
            CancellationToken token)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var client = GetClient();
            return action(client, token);
        }

        private T GetClient()
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