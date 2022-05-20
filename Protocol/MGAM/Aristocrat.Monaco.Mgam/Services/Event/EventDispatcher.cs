namespace Aristocrat.Monaco.Mgam.Services.Event
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Consumers;
    using Kernel;
    using SimpleInjector;

    /// <summary>
    ///     Subscribes event consumers to events.
    /// </summary>
    public sealed class EventDispatcher : IEventDispatcher, IDisposable
    {
        private readonly Container _container;
        private readonly IEventBus _eventBus;
        private readonly ISharedConsumer _consumerContext;
        private readonly StartupEventListener _eventListener;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventDispatcher"/> class.
        /// </summary>
        /// <param name="container">Instance of <see cref="Container"/>.</param>
        /// <param name="eventBus">Instance of <see cref="IEventBus"/>.</param>
        /// <param name="consumerContext">Instance of <see cref="ISharedConsumer"/>.</param>
        /// <param name="eventListener">Instance of <see cref="StartupEventListener"/>.</param>
        public EventDispatcher(Container container, IEventBus eventBus, ISharedConsumer consumerContext, StartupEventListener eventListener)
        {
            _container = container;
            _eventBus = eventBus;
            _consumerContext = consumerContext;
            _eventListener = eventListener;

            SubscribeToEvents();
        }

        /// <inheritdoc />
        ~EventDispatcher()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public Type[] ConsumedEventTypes { get; private set; }

        public void Unsubscribe()
        {
            _eventBus.UnsubscribeAll(_consumerContext);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Unsubscribe();
            }

            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            SubscribeConsumers();
        }

        private void SubscribeConsumers()
        {
            ConsumedEventTypes =
            (
                from type in Assembly.GetExecutingAssembly().GetExportedTypes()
                from itf in type.GetInterfaces()
                from evt in itf.GetGenericArguments()
                where itf.GetGenericTypeDefinition() == typeof(IConsumer<>) && typeof(IEvent).IsAssignableFrom(evt) && !evt.IsGenericParameter
                select evt
            ).ToArray();

            foreach (var et in ConsumedEventTypes)
            {
                var handler = BuildHandler(et);

                _eventBus.Subscribe(
                    _consumerContext,
                    et,
                    async (evt, ct) =>
                    {
                        foreach (var consumer in _container.GetAllInstances(typeof(IConsumer<>).MakeGenericType(et)))
                        {
                            await handler.Invoke(consumer, evt, ct);
                        }
                    });

                _eventListener.Subscribe(
                    et,
                    evt =>
                    {
                        foreach (var consumer in _container.GetAllInstances(typeof(IConsumer<>).MakeGenericType(et)))
                        {
                            handler.Invoke(consumer, evt, CancellationToken.None).Wait();
                        }
                    });
            }
        }

        private static Func<object, IEvent, CancellationToken, Task> BuildHandler(Type eventType)
        {
            var consumerExpr = Expression.Parameter(typeof(object), "consumer");
            var evtExpr = Expression.Parameter(typeof(IEvent), "evt");
            var tokenExpr = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            var method = typeof(Consumers.Consumes<>).MakeGenericType(eventType).GetMethod(@"Consume", new[] { eventType, typeof(CancellationToken) });
            Debug.Assert(method != null);

            var methodExpr = Expression.Call(
                Expression.Convert(consumerExpr, typeof(Consumers.Consumes<>).MakeGenericType(eventType)),
                method,
                new Expression[] { Expression.Convert(evtExpr, eventType), tokenExpr });

            var lambdaExpr = Expression.Lambda<Func<object, IEvent, CancellationToken, Task>>(
                methodExpr, consumerExpr, evtExpr, tokenExpr);

            return lambdaExpr.Compile();
        }
    }
}
