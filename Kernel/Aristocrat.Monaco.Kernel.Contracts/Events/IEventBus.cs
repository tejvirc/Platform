namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     This interface allows components to subscribe to and receive events.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         It is important that all subscribers cancel their event subscriptions when they are no
    ///         longer needed.  Subscribers can call Unsubscribe() for each subscribed event type, or
    ///         call UnsubscribeAll().  Subscribers should expect that their event queue will be
    ///         destroyed once all subscriptions are canceled, even if the queue is not empty.
    ///     </para>
    /// </remarks>
    /// <seealso cref="IEvent" />
    public interface IEventBus
    {
        /// <summary>
        ///     The method used to post an event to the Event Bus
        /// </summary>
        /// <typeparam name="T">The type of event to publish</typeparam>
        /// <param name="event">The event to be published</param>
        void Publish<T>(T @event)
            where T : IEvent;

        /// <summary>
        ///     Used to subscribe to events and specify a delegate that will be called when events
        ///     of the specified type are received
        /// </summary>
        /// <typeparam name="T">The type of event to subscribe to</typeparam>
        /// <param name="context">
        ///     Defines the subscriber context, which will use the same thread pool effectively guaranteeing
        ///     ordered delivery of events for the provided context
        /// </param>
        /// <param name="handler">The delegate to call when this type of event is received</param>
        void Subscribe<T>(object context, Action<T> handler)
            where T : IEvent;

        /// <summary>
        ///     Used to subscribe to events and specify a delegate that will be called when events
        ///     of the specified type are received
        /// </summary>
        /// <typeparam name="T">The type of event to subscribe to</typeparam>
        /// <param name="context">
        ///     Defines the subscriber context, which will use the same thread pool effectively guaranteeing
        ///     ordered delivery of events for the provided context
        /// </param>
        /// <param name="handler">The delegate to call when this type of event is received</param>
        void Subscribe<T>(object context, Func<T, CancellationToken, Task> handler)
            where T : IEvent;

        /// <summary>
        ///     Used to subscribe to events and specify a delegate that will be called when events
        ///     of the specified type are received
        /// </summary>
        /// <typeparam name="T">The type of event to subscribe to</typeparam>
        /// <param name="context">
        ///     Defines the subscriber context, which will use the same thread pool effectively guaranteeing
        ///     ordered delivery of events for the provided context
        /// </param>
        /// <param name="handler">The delegate to call when this type of event is received</param>
        /// <param name="filter">
        ///     A function to test the event for a condition. A return value of false will result in the event not
        ///     being published to the subscriber context.
        /// </param>
        void Subscribe<T>(object context, Action<T> handler, Predicate<T> filter)
            where T : IEvent;

        /// <summary>
        ///     Used to subscribe to events and specify a delegate that will be called when events
        ///     of the specified type are received
        /// </summary>
        /// <typeparam name="T">The type of event to subscribe to</typeparam>
        /// <param name="context">
        ///     Defines the subscriber context, which will use the same thread pool effectively guaranteeing
        ///     ordered delivery of events for the provided context
        /// </param>
        /// <param name="handler">The delegate to call when this type of event is received</param>
        /// <param name="filter">
        ///     A function to test the event for a condition. A return value of false will result in the event not
        ///     being published to the subscriber context.
        /// </param>
        void Subscribe<T>(object context, Func<T, CancellationToken, Task> handler, Predicate<T> filter)
            where T : IEvent;

        /// <summary>
        ///     Used to unsubscribe to events of this type
        /// </summary>
        /// <param name="context">Defines the subscriber context</param>
        /// <typeparam name="T">The type of event to unsubscribe from</typeparam>
        void Unsubscribe<T>(object context)
            where T : IEvent;

        /// <summary>
        ///     Used to unsubscribe to all events.
        /// </summary>
        /// <param name="context">Defines the subscriber context</param>
        void UnsubscribeAll(object context);
    }
}