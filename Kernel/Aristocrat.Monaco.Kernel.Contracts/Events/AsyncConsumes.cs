namespace Aristocrat.Monaco.Kernel.Contracts.Events
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A user-friendly event receiver
    /// </summary>
    /// <typeparam name="TEvent">The event to handle</typeparam>
    public abstract class AsyncConsumes<TEvent> : IAsyncConsumer<TEvent>, IDisposable
        where TEvent : BaseEvent
    {
        private readonly object _consumerContext;
        private readonly IEventBus _eventBus;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Consumes{TEvent}" /> class.
        /// </summary>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="consumerContext">The object reference of the event subscription.</param>
        /// <param name="filter">
        ///     A function to test the event for a condition. A return value of false will result in the event not
        ///     being published to the subscriber context.
        /// </param>
        protected AsyncConsumes(IEventBus eventBus, object consumerContext = null, Predicate<TEvent> filter = null)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _consumerContext = consumerContext ?? this;
            _eventBus.Subscribe(_consumerContext, Consume, filter);
        }

        /// <inheritdoc />
        public abstract Task Consume(TEvent theEvent, CancellationToken token);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if dispose should be called on managed objects.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.Unsubscribe<TEvent>(_consumerContext);
            }

            _disposed = true;
        }
    }
}