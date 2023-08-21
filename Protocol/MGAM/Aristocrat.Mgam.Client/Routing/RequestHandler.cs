namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Logging;
    using Messaging;
    using SimpleInjector;

    /// <summary>
    ///     Dispatches requests to message handlers.
    /// </summary>
    internal sealed class RequestHandler : IRequestHandler, IDisposable
    {
        private readonly ILogger _logger;
        private readonly Container _container;

        private readonly ConcurrentDictionary<Type, InstanceProducer> _producers = new ConcurrentDictionary<Type, InstanceProducer>();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RequestHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="container">Dependency injection container.</param>
        public RequestHandler(
            ILogger<RequestHandler> logger,
            Container container)
        {
            _logger = logger;
            _container = container;
        }

        /// <inheritdoc />
        ~RequestHandler()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public async Task<IResponse> Receive<TRequest>(TRequest request) where TRequest : IRequest
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                _logger.LogDebug("Receiving {0} message.", request.GetType().Name);

                var requestType = request.GetType();

                if (!_producers.TryGetValue(requestType, out var producer))
                {
                    producer = _container.GetRegistration(typeof(IMessageHandler<>).MakeGenericType(request.GetType()));
                    if (producer == null)
                    {
                        throw new InvalidOperationException($"No handler found for {requestType.Name} request");
                    }

                    _producers.TryAdd(requestType, producer);
                }

                var handler = producer.GetInstance() as dynamic;

                var response = (IResponse)await handler.Handle(request as dynamic);

                _logger.LogDebug(
                    "Receive {0} message succeeded -- returning {1} message with response code {2}.",
                    request.GetType().Name,
                    response?.GetType().Name ?? @"No Response",
                    response?.ResponseCode.ToString() ?? @"None");

                return response;
            }
            catch (AggregateException ex)
            {
                foreach (var exception in ex.InnerExceptions)
                {
                    _logger.LogError(exception, "Receive {0} message failed", request.GetType().Name);
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Receive {0} message failed", request.GetType().Name);
                throw;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("ReSharper", "UseNullPropagation")]
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_producers != null)
                {
                    _producers.Clear();
                }
            }

            _disposed = true;
        }
    }
}
