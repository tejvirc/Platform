namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Polly;

    public abstract class BaseClientCommunicationService<T> where T : ClientBase<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(100);
        private const int RetryCount = 3;
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
            Invoke(async (c, _, _) => await action(c).ConfigureAwait(false), (object)null, policy, CancellationToken.None);

        protected Task<TResult> Invoke<TResult>(
            Func<T, CancellationToken, Task<TResult>> action,
            CancellationToken token) =>
            Invoke(async (c, _, t) => await action(c, t).ConfigureAwait(false), (object)null, null, token);

        protected Task<TResult> Invoke<TResult>(
            Func<T, CancellationToken, Task<TResult>> action,
            IAsyncPolicy policy,
            CancellationToken token) =>
            Invoke(async (c, _, t) => await action(c, t).ConfigureAwait(false), (object)null, policy, token);

        protected Task<TResult> Invoke<TResult, TMessage>(
            Func<T, TMessage, CancellationToken, Task<TResult>> action,
            TMessage message,
            CancellationToken token) =>
            Invoke(action, message, null, token);

        protected Task<TResult> Invoke<TResult, TMessage>(
            Func<T, TMessage, CancellationToken, Task<TResult>> action,
            TMessage message,
            IAsyncPolicy policy,
            CancellationToken token)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return InvokeInternal(action, message, policy, token);
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
            Invoke((c, _, _) => action(c), (object)null, CancellationToken.None);
        protected AsyncServerStreamingCall<TOutput> Invoke<TOutput>(
            Func<T, CancellationToken, AsyncServerStreamingCall<TOutput>> action,
            CancellationToken token) =>
            Invoke((c, _, t) => action(c, t), (object)null, token);

        protected AsyncServerStreamingCall<TOutput> Invoke<TOutput, TMessage>(
            Func<T, TMessage, CancellationToken, AsyncServerStreamingCall<TOutput>> action,
            TMessage message,
            CancellationToken token)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var client = GetClient();
            return action(client, message, token);
        }

        private async Task<TResult> InvokeInternal<TResult, TMessage>(
            Func<T, TMessage, CancellationToken, Task<TResult>> action,
            TMessage message,
            IAsyncPolicy policy,
            CancellationToken token)
        {
            var client = GetClient();
            if (policy is null)
            {
                return await action(client, message, token).ConfigureAwait(false);
            }

            return await policy.ExecuteAsync(async t => await action(client, message, t).ConfigureAwait(false), token)
                .ConfigureAwait(false);
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