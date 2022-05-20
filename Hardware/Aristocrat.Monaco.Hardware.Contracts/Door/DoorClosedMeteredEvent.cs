namespace Aristocrat.Monaco.Hardware.Contracts.Door
{
    using System;

    /// <summary>Definition of the DoorClosedMeteredEvent class.</summary>
    [Serializable]
    public class DoorClosedMeteredEvent : DoorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorClosedMeteredEvent" /> class.
        /// </summary>
        public DoorClosedMeteredEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorClosedMeteredEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical door ID.</param>
        /// <param name="doorName">The name of the door.</param>
        public DoorClosedMeteredEvent(int logicalId, string doorName)
            : base(logicalId, false, doorName)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorClosedMeteredEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical door ID.</param>
        /// <param name="whilePoweredDown">Indicates if event occurred while the EGM was powered down.</param>
        /// <param name="doorName">The name of the door.</param>
        public DoorClosedMeteredEvent(int logicalId, bool whilePoweredDown, string doorName)
            : base(logicalId, whilePoweredDown, doorName)
        {
        }
    }
}