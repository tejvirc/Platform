namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Grpc.Core;
    using log4net;

    public class BingoClientClientStreamingLogger<TRequest> : IClientStreamWriter<TRequest>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IClientStreamWriter<TRequest> _caller;

        public BingoClientClientStreamingLogger(IClientStreamWriter<TRequest> caller)
        {
            _caller = caller ?? throw new ArgumentNullException(nameof(caller));
        }

        public Task WriteAsync(TRequest message)
        {
            Logger.Debug($"Sending Request: {message}");
            return _caller.WriteAsync(message);
        }

        public WriteOptions WriteOptions
        {
            get => _caller.WriteOptions;
            set => _caller.WriteOptions = value;
        }

        public Task CompleteAsync() => _caller.CompleteAsync();
    }
}