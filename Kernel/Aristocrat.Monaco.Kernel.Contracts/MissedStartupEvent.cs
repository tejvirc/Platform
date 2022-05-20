namespace Aristocrat.Monaco.Kernel
{
    using System;

    /// <summary>
    ///     This event signals missed important events for state on startup and allows for their processing
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When any component has completely booted up it can subscribe to this
    ///         event and perform the necessary action
    ///     </para>
    /// </remarks>
    [Serializable]
    public class MissedStartupEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MissedStartupEvent" /> class.
        /// </summary>
        /// <param name="missedEvent">The event that was missed</param>
        public MissedStartupEvent(IEvent missedEvent)
        {
            MissedEvent = missedEvent;
        }

        /// <summary>
        ///     Gets the missed event
        /// </summary>
        public IEvent MissedEvent;
    }
}