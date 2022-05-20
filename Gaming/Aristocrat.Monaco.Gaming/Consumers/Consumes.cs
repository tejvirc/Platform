namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Kernel;

    /// <summary>A user-friendly event receiver</summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    public abstract class Consumes<TEvent> : Kernel.Consumes<TEvent>
        where TEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Consumes{TEvent}" /> class.
        /// </summary>
        protected Consumes()
            : base(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<ISharedConsumer>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Consumes{TEvent}" /> class.
        /// </summary>
        /// <param name="filter">
        ///     A function to test the event for a condition. A return value of false will result in the event not
        ///     being published to the subscriber context.
        /// </param>
        protected Consumes(Predicate<TEvent> filter = null)
            : base(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<ISharedConsumer>(),
                filter)
        {
        }
    }
}
