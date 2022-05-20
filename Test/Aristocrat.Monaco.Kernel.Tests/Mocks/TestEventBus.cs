namespace Aristocrat.Monaco.Kernel.Tests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Kernel.Events;

    public sealed class TestEventBus : IEventBus, IService
    {
        private readonly Dictionary<Type, Subscription> _subscriptions = new Dictionary<Type, Subscription>();

        public void Publish<T>(T @event) where T : IEvent
        {
            if (_subscriptions.TryGetValue(typeof(T), out var subscription))
            {
                subscription.Handler.Invoke(@event, CancellationToken.None);
            }
        }

        public void Subscribe<T>(object context, Action<T> handler) where T : IEvent
        {
            Subscribe<T>(
                context,
                (message, cancellationToken) =>
                {
                    handler.Invoke(message);
                    return Task.FromResult(0);
                });
        }

        public void Subscribe<T>(object context, Func<T, CancellationToken, Task> handler) where T : IEvent
        {
            if (!_subscriptions.ContainsKey(typeof(T)))
                _subscriptions.Add(typeof(T), Subscription.Create(handler, null));
        }

        public void Subscribe<T>(object context, Action<T> handler, Predicate<T> filter) where T : IEvent
        {
            Subscribe<T>(
                context,
                (message, cancellationToken) =>
                {
                    handler.Invoke(message);
                    return Task.FromResult(0);
                },
                filter);
        }

        public void Subscribe<T>(object context, Func<T, CancellationToken, Task> handler, Predicate<T> filter) where T : IEvent
        {
            if (!_subscriptions.ContainsKey(typeof(T)))
                _subscriptions.Add(typeof(T), Subscription.Create(handler, filter));
        }

        public void Unsubscribe<T>(object subscriber) where T : IEvent
        {
            _subscriptions.Remove(typeof(T));
        }

        public void UnsubscribeAll(object subscriber)
        {
            _subscriptions.Clear();
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IEventBus) };

        public void Initialize()
        {
        }
    }
}