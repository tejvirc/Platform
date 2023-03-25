namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;
    using System.Threading.Tasks;
    using Grpc.Core;
    using log4net;

    public class BingoClientClientStreamingLogger<TRequest> : IClientStreamWriter<TRequest>
    {
        private readonly ILog _logger;
        private readonly IClientStreamWriter<TRequest> _caller;

        public BingoClientClientStreamingLogger(ILog logger, IClientStreamWriter<TRequest> caller)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _caller = caller ?? throw new ArgumentNullException(nameof(caller));
        }

        public Task WriteAsync(TRequest message)
        {
            _logger.Debug($"Sending Request: {message}");
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