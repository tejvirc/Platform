namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using log4net;

    public class ClientServerStreamingLogger<TRequest> : IAsyncStreamReader<TRequest>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IAsyncStreamReader<TRequest> _caller;
        private readonly Action _action;

        public ClientServerStreamingLogger(IAsyncStreamReader<TRequest> caller, Action messageReceived)
        {
            _caller = caller ?? throw new ArgumentNullException(nameof(caller));
            _action = messageReceived;
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            var hasNext = await _caller.MoveNext(cancellationToken);
            if (hasNext)
            {
                Logger.Debug($"Received Response: {Current}");
                _action.Invoke();
            }

            return hasNext;
        }

        public TRequest Current => _caller.Current;
    }
}