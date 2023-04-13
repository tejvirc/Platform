namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using log4net;

    public class BingoClientServerStreamingLogger<TRequest> : IAsyncStreamReader<TRequest>
    {
        private readonly ILog _logger;
        private readonly IAsyncStreamReader<TRequest> _caller;
        private readonly Action _onMessageReceived;
        private readonly Action<RpcException> _rpcExceptionOccurred;

        public BingoClientServerStreamingLogger(
            ILog logger,
            IAsyncStreamReader<TRequest> caller,
            Action messageReceived,
            Action<RpcException> rpcExceptionOccurred)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _caller = caller ?? throw new ArgumentNullException(nameof(caller));
            _onMessageReceived = messageReceived ?? throw new ArgumentNullException(nameof(messageReceived));
            _rpcExceptionOccurred =
                rpcExceptionOccurred ?? throw new ArgumentNullException(nameof(rpcExceptionOccurred));
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            try
            {
                var hasNext = await _caller.MoveNext(cancellationToken).ConfigureAwait(false);
                if (!hasNext)
                {
                    return false;
                }

                _logger.Debug($"Received Response: {Current}");
                _onMessageReceived();
                return true;
            }
            catch (RpcException rpcException)
            {
                _rpcExceptionOccurred(rpcException);
                throw;
            }
        }

        public TRequest Current => _caller.Current;
    }
}