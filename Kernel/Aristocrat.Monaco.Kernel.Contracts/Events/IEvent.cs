namespace Aristocrat.Monaco.Kernel
{
    using System;

    /// <summary>
    ///     Provides the interface for all events posted to the Event Bus
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All events should be Serializable and have a default constructor for use in tools.
    ///     </para>
    ///     <para>
    ///         The <see cref="BaseEvent" /> class provides a base implementation from which most events
    ///         can and should derive.
    ///     </para>
    ///     <para>
    ///         All events should override ToString() in a manner consistent with the implementation
    ///         in <see cref="BaseEvent" />.  The ToString() implementation should include the values of all
    ///         possible fields and properties.
    ///     </para>
    /// </remarks>
    public interface IEvent
    {
        /// <summary>
        ///     Gets the Guid for this event
        /// </summary>
        Guid GloballyUniqueId { get; }
    }
}