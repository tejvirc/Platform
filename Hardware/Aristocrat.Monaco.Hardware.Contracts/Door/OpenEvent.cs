namespace Aristocrat.Monaco.Hardware.Contracts.Door
{
    using System;

    /// <summary>Definition of the DoorOpenEvent class.</summary>
    [Serializable]
    public class OpenEvent : DoorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OpenEvent" /> class.
        /// </summary>
        public OpenEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OpenEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical door ID.</param>
        /// <param name="doorName">The name of the door.</param>
        public OpenEvent(int logicalId, string doorName)
            : base(logicalId, false, doorName)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OpenEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical door ID.</param>
        /// <param name="whilePoweredDown">Indicates if event occurred while the EGM was powered down.</param>
        /// <param name="doorName">The name of the door.</param>
        public OpenEvent(int logicalId, bool whilePoweredDown, string doorName)
            : base(logicalId, whilePoweredDown, doorName)
        {
        }
    }
}