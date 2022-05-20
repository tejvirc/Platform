namespace Aristocrat.Monaco.Kernel
{
    /// <summary>
    ///     A user-friendly event receiver
    /// </summary>
    /// <typeparam name="TEvent">The event to handle</typeparam>
    public interface IConsumer<in TEvent> : IConsumer
        where TEvent : IEvent
    {
        /// <summary>
        ///     Consumes an event
        /// </summary>
        /// <param name="theEvent">The event to consume</param>
        void Consume(TEvent theEvent);
    }

    /// <summary>
    ///     Marker interface used to assist identification in IoC containers. Not to be used directly.
    /// </summary>
    public interface IConsumer
    {
    }
}