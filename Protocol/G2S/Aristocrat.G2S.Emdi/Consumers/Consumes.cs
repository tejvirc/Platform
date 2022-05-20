namespace Aristocrat.G2S.Emdi.Consumers
{
    using Monaco.Kernel;
    using System;
    using System.Threading.Tasks;

    /// <summary>A user-friendly event receiver</summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    public abstract class Consumes<TEvent> : IEmdiConsumer<TEvent>, IDisposable
        where TEvent : BaseEvent
    {
        private readonly IEventBus _eventBus;
        private readonly object _consumerContext;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Monaco.Kernel.Consumes{TEvent}" /> class.
        /// </summary>
        protected Consumes()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<ISharedConsumer>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Monaco.Kernel.Consumes{TEvent}" /> class.
        /// </summary>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="consumerContext">The object reference of the event subscription.</param>
        private Consumes(IEventBus eventBus, object consumerContext)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _consumerContext = consumerContext ?? this;
            _eventBus.Subscribe<TEvent>(_consumerContext, Consume);
        }

        /// <inheritdoc />
        public void Consume(TEvent theEvent)
        {
            ConsumeAsync(theEvent).Wait();
        }

        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="theEvent"></param>
        /// <returns></returns>
        protected abstract Task ConsumeAsync(TEvent theEvent);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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
