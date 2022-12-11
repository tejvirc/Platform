namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Globalization;
    using ProtoBuf;

    /// <summary>
    ///     An event implementation that provides the minimal implementation to have a
    ///     Guid and provide access to it via the IEvent::GloballyUniqueId property.
    ///     The class also implements a Timestamp property which provides date and time
    ///     of object construction.
    /// </summary>
    /// <inheritdoc />
    [ProtoContract]
    [Serializable]
    public abstract class BaseEvent : IEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseEvent" /> class.
        /// </summary>
        protected BaseEvent()
        {
            GloballyUniqueId = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        ///     Gets date and time when this object was constructed
        /// </summary>
        [ProtoMember(1)]
        public DateTime Timestamp { get; }

        /// <inheritdoc />
        [ProtoMember(2)]
        public Guid GloballyUniqueId { get; }

        /// <summary>
        ///     Creates a string representation of the event's data. Override this in
        ///     all derived event types that add additional fields and/or properties.
        /// </summary>
        /// <returns>The string representation of the event</returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"{GetType().Name}");
        }
    }
}