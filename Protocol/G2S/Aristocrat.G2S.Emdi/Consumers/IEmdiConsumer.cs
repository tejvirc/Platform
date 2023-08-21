namespace Aristocrat.G2S.Emdi.Consumers
{
    using Monaco.Kernel;

    /// <summary>
    ///     A user-friendly event receiver
    /// </summary>
    /// <typeparam name="TEvent">The event to handle</typeparam>
    public interface IEmdiConsumer<in TEvent> : IConsumer
        where TEvent : IEvent
    {
        /// <summary>
        ///     Consumes an event
        /// </summary>
        /// <param name="theEvent">The event to consume</param>
        void Consume(TEvent theEvent);
    }
}