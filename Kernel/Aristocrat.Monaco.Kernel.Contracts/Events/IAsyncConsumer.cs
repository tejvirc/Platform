namespace Aristocrat.Monaco.Kernel.Contracts.Events
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A user-friendly event receiver
    /// </summary>
    /// <typeparam name="TEvent">The event to handle</typeparam>
    public interface IAsyncConsumer<in TEvent> : IConsumer
        where TEvent : BaseEvent
    {
        /// <summary>
        ///     Consumes an event
        /// </summary>
        /// <param name="theEvent">The event to consume</param>
        /// <param name="token">The cancellation token for this task</param>
        Task Consume(TEvent theEvent, CancellationToken token);
    }
}