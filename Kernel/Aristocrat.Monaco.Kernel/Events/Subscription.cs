namespace Aristocrat.Monaco.Kernel.Events
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents an event subscription
    /// </summary>
    internal sealed class Subscription
    {
        private Subscription(Func<IEvent, CancellationToken, Task> handler, Predicate<IEvent> filter)
        {
            Handler = handler;
            Filter = filter;
        }

        /// <summary>
        ///     Gets the event handler
        /// </summary>
        public Func<IEvent, CancellationToken, Task> Handler { get; }

        /// <summary>
        ///     A function to test the event for a condition. A return value of false will result in the event not
        ///     being published to the subscriber context.
        /// </summary>
        public Predicate<IEvent> Filter { get; set; }

        /// <summary>
        ///     Creates a subscription
        /// </summary>
        /// <typeparam name="T">The event type</typeparam>
        /// <param name="handler">The event handler</param>
        /// <param name="filter">
        ///     A function to test the event for a condition. A return value of false will result in the event not
        ///     being published to the subscriber context.
        /// </param>
        /// <returns>A <see cref="Subscription" /></returns>
        public static Subscription Create<T>(Func<T, CancellationToken, Task> handler, Predicate<T> filter)
            where T : IEvent
        {
            return new Subscription(
                async (message, cancellationToken) => { await handler.Invoke((T)message, cancellationToken); }, evt => filter?.Invoke((T)evt) ?? true);
        }
    }
}