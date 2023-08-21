namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Kernel;

    /// <summary>
    ///     A user-friendly event receiver.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    public abstract class Consumes<TEvent> : IConsumer<TEvent>
        where TEvent : IEvent
    {
        /// <inheritdoc />
        public void Consume(TEvent theEvent)
        {
            Consume(theEvent, CancellationToken.None).Wait();
        }

        /// <summary>
        ///     Consumes an event
        /// </summary>
        /// <param name="theEvent">The event to consume</param>
        /// <param name="cancellationToken"></param>
        public abstract Task Consume(TEvent theEvent, CancellationToken cancellationToken);
    }
}
