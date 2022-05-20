namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Kernel;

    /// <summary>A user-friendly event receiver</summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    public abstract class Consumes<TEvent> : Kernel.Consumes<TEvent>
        where TEvent : BaseEvent
    {
        /// <inheritdoc />
        protected Consumes(Predicate<TEvent> filter = null)
            : base(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<ISharedConsumer>(),
                filter)
        {
        }
    }
}