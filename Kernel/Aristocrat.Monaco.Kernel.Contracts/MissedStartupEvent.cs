namespace Aristocrat.Monaco.Kernel
{
    using ProtoBuf;
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
    [ProtoContract]
    public class MissedStartupEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public MissedStartupEvent()
        {
        }

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
        [ProtoMember(1)]
        public IEvent MissedEvent;
    }
}