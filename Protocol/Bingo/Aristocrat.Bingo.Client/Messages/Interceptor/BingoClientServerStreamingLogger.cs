namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using log4net;

    public class BingoClientServerStreamingLogger<TRequest> : IAsyncStreamReader<TRequest>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAsyncStreamReader<TRequest> _caller;

        private readonly BingoClientInterceptor _bingoClientInterceptor;

        public BingoClientServerStreamingLogger(IAsyncStreamReader<TRequest> caller, BingoClientInterceptor bingoClientInterceptor)
        {
            _caller = caller ?? throw new ArgumentNullException(nameof(caller));
            _bingoClientInterceptor =
                bingoClientInterceptor ?? throw new ArgumentNullException(nameof(bingoClientInterceptor));
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            var hasNext = await _caller.MoveNext(cancellationToken);
            if (hasNext)
            {
                Logger.Debug($"Received Response: {Current}");
                _bingoClientInterceptor.OnMessageReceived();
            }

            return hasNext;
        }

        public TRequest Current => _caller.Current;
    }
}