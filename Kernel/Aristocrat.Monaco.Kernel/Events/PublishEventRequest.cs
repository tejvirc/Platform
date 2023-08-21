namespace Aristocrat.Monaco.Kernel.Events
{
    using System.Threading;

    /// <summary>
    ///     Represents a request to publish an event
    /// </summary>
    internal sealed class PublishEventRequest
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PublishEventRequest" /> class.
        /// </summary>
        /// <param name="event">The event</param>
        /// <param name="cancellationToken">A cancellation token</param>
        public PublishEventRequest(IEvent @event, CancellationToken cancellationToken)
        {
            Event = @event;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        ///     Gets the event to publish
        /// </summary>
        public IEvent Event { get; }

        /// <summary>
        ///     Gets the cancellation token
        /// </summary>
        public CancellationToken CancellationToken { get; }
    }
}