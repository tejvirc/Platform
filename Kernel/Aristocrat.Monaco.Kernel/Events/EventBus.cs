namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Events;
    using log4net;

    /// <inheritdoc cref="IEventBus" />
    public sealed class EventBus : IEventBus, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ConcurrentDictionary<object, Subscriber> _subscribers =
            new ConcurrentDictionary<object, Subscriber>();

        private bool _disposed;

        /// <inheritdoc />
        ~EventBus()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Publish<T>(T @event)
            where T : IEvent
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            Logger.Debug($"Publishing event {@event} to subscribers");

            var subscribers = _subscribers.Values.Where(s => s.HandlesEvent(@event));
            foreach (var subscriber in subscribers)
            {
                subscriber.Publish(new PublishEventRequest(@event, CancellationToken.None));
            }
        }

        /// <inheritdoc />
        public void Subscribe<T>(object context, Action<T> handler)
            where T : IEvent
        {
            Subscribe(context, handler, null);
        }

        /// <inheritdoc />
        public void Subscribe<T>(object context, Func<T, CancellationToken, Task> handler)
            where T : IEvent
        {
            Subscribe(context, handler, null);
        }

        /// <inheritdoc />
        public void Subscribe<T>(object context, Action<T> handler, Predicate<T> filter) where T : IEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Subscribe(
                context,
                (message, cancellationToken) =>
                {
                    handler.Invoke(message);
                    return Task.CompletedTask;
                },
                filter);
        }

        /// <inheritdoc />
        public void Subscribe<T>(object context, Func<T, CancellationToken, Task> handler, Predicate<T> filter) where T : IEvent
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var subscriber = _subscribers.GetOrAdd(context, _ => new Subscriber(context.GetType().FullName));

            subscriber.Subscribe(handler, filter);

            Logger.Debug($"{context} subscribed to event {typeof(T)}");
        }

        /// <inheritdoc />
        public void Unsubscribe<T>(object context)
            where T : IEvent
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_subscribers.TryGetValue(context, out var subscriber))
            {
                // Returns the subscription count.  If it's zero we're going to remove the subscriber
                if (subscriber.UnSubscribe<T>() == 0)
                {
                    if (_subscribers.TryRemove(context, out _))
                    {
                        // This is being run asynchronously since this is typically called from the same thread that's handling the event
                        //  This is lieu of a broader solution like leveraging System.Reactive
                        Task.Run(() => subscriber.Dispose());
                    }
                }
            }

            Logger.Debug($"{context} unsubscribed from event {typeof(T)}");
        }

        /// <inheritdoc />
        public void UnsubscribeAll(object context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_subscribers.TryRemove(context, out var subscriber))
            {
                subscriber.Dispose();
            }

            Logger.Debug($"{context} unsubscribed from all events");
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IEventBus) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var subscriber in _subscribers.Values)
                {
                    subscriber.Dispose();
                }

                _subscribers.Clear();
            }

            _disposed = true;
        }
    }
}
