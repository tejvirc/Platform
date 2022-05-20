namespace Aristocrat.Monaco.Mgam.Services.Event
{
    using System;

    /// <summary>
    ///     Subscribes event consumers to events.
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        ///     Get an array of Type of events that have consumers.
        /// </summary>
        Type[] ConsumedEventTypes { get; }

        /// <summary>
        ///     Unsubscribe consumers from all events.
        /// </summary>
        void Unsubscribe();
    }
}
