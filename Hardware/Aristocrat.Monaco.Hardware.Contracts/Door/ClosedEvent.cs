namespace Aristocrat.Monaco.Hardware.Contracts.Door
{
    using System;

    /// <summary>Definition of the DoorClosedEvent class.</summary>
    [Serializable]
    public class ClosedEvent : DoorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClosedEvent" /> class.
        /// </summary>
        public ClosedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClosedEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical door ID.</param>
        /// <param name="doorName">The name of the door.</param>
        public ClosedEvent(int logicalId, string doorName)
            : base(logicalId, false, doorName)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClosedEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical door ID.</param>
        /// <param name="whilePoweredDown">Indicates if event occurred while the EGM was powered down.</param>
        /// <param name="doorName">The name of the door.</param>
        public ClosedEvent(int logicalId, bool whilePoweredDown, string doorName)
            : base(logicalId, whilePoweredDown, doorName)
        {
        }
    }
}