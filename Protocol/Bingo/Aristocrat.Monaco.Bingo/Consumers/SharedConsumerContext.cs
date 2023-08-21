namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Collections.Generic;
    using Kernel;

    public class SharedConsumerContext : ISharedConsumer, IDisposable
    {
        private bool _disposed;
        private readonly IEventBus _eventBus;

        public SharedConsumerContext(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(ISharedConsumer) };

        public void Initialize()
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}
