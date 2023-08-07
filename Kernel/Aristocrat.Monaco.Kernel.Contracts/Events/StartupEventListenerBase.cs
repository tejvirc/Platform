namespace Aristocrat.Monaco.Kernel.Contracts.Events
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Kernel;
    using log4net;

    /// <summary>
    ///     A base class which can be inherited to save and handle the early events
    /// </summary>
    public abstract class StartupEventListenerBase : IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ConcurrentQueue<IEvent> _events = new ConcurrentQueue<IEvent>();

        private bool _disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Gets or sets the events bus.</summary>
        public IEventBus EventBus { get; set; }

        /// <inheritdoc />
        public virtual string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { GetType() };

        /// <inheritdoc />
        public void Initialize()
        {
            Subscribe();
        }

        /// <summary>
        ///     Unsubscribe from all events
        /// </summary>
        public void Unsubscribe()
        {
            EventBus?.UnsubscribeAll(this);
        }

        /// <summary>A method which must be inherited to subscribe the interested events</summary>
        protected abstract void Subscribe();

        /// <summary>
        ///     Checks if any event meeting the condition exits
        /// </summary>
        /// <typeparam name="TEvent">the type of event.</typeparam>
        /// <param name="predicate">the predicate used in searching.</param>
        /// <returns></returns>
        public bool HasEvent<TEvent>(Expression<Func<TEvent, bool>> predicate)
            where TEvent : IEvent
        {
            return _events.AsQueryable().Where(e => e is TEvent).Cast<TEvent>().Any(predicate);
        }

        /// <summary>
        ///     Handles the startup events
        /// </summary>
        /// <param name="getConsumers">The delegate to get consumers</param>
        public void HandleStartupEvents(Func<Type, dynamic> getConsumers)
        {
            IEvent data;
            while ((data = Dequeue()) != null)
            {             
                ConsumeEvent(data, getConsumers);
            }

            Unsubscribe();
        }

        /// <summary>
        /// ConsumeEvent
        /// </summary>
        /// <param name="data"></param>
        /// <param name="getConsumers"></param>
        protected virtual void ConsumeEvent(IEvent data, Func<Type, dynamic> getConsumers)
        {
            var genericConsumer = typeof(IConsumer<>);
            Type[] typeArgs = { data.GetType() };
            var consumer = genericConsumer.MakeGenericType(typeArgs);
            getConsumers(consumer)?.Consume((dynamic)data);
        }

        /// <summary>The queue to save the early events</summary>
        protected virtual ConcurrentQueue<IEvent> EventQueue => _events;

        /// <summary>Disposes the resources used in this object</summary>
        /// <param name="disposing">indicates whether to dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Unsubscribe();
                while (_events.TryDequeue(out var _)) { }
            }

            _disposed = true;
        }

        private IEvent Dequeue()
        {
            if (_events.TryDequeue(out var data))
            {
                Logger.Info($"Dequeued event: {data}");

                return data;
            }

            return null;
        }
    }
}
