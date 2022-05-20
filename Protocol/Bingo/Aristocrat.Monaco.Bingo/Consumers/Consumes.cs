namespace Aristocrat.Monaco.Bingo.Consumers
{
        using System;
        using Kernel;

        /// <summary>A user-friendly event receiver</summary>
        /// <typeparam name="TEvent">The event type</typeparam>
        public abstract class Consumes<TEvent> : Kernel.Consumes<TEvent>
            where TEvent : BaseEvent
        {
            /// <inheritdoc />
            protected Consumes(IEventBus eventBus, object consumerContext = null, Predicate<TEvent> filter = null)
                : base(eventBus, consumerContext, filter)
            {
            }
        }
}