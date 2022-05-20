namespace Aristocrat.Monaco.Kernel
{
    using System;

    /// <summary>
    ///     This event signals that the platform has booted up. This is posted by the <c>Application.BootExtender</c>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The components that need to perform actions after the platform is completely booted up can subscribe to this
    ///         event and perform the necessary action
    ///     </para>
    /// </remarks>
    [Serializable]
    public class PlatformBootedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlatformBootedEvent" /> class.
        /// </summary>
        /// <remarks>
        ///     Default constructor necessary for serialization
        /// </remarks>
        public PlatformBootedEvent()
            : this(DateTime.UtcNow, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlatformBootedEvent" /> class.
        /// </summary>
        /// <param name="time">The time the event occurred</param>
        public PlatformBootedEvent(DateTime time)
            : this(time, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlatformBootedEvent" /> class.
        /// </summary>
        /// <param name="time">The time the event occurred</param>
        /// <param name="criticalMemoryCleared">Denotes whether or not persistent storage was cleared before startup.</param>
        public PlatformBootedEvent(DateTime time, bool criticalMemoryCleared)
        {
            Time = time;
            CriticalMemoryCleared = criticalMemoryCleared;
        }

        /// <summary>
        ///     Gets the time the PlatformBootedEvent happened
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        ///     Gets a value indicating whether or not critical memory was cleared
        /// </summary>
        public bool CriticalMemoryCleared { get; }
    }
}