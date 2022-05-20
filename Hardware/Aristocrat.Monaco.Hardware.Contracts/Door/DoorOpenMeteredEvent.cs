namespace Aristocrat.Monaco.Hardware.Contracts.Door
{
    /// <summary>
    ///     This event is used to signal that a door open event has been metered.
    /// </summary>
    public class DoorOpenMeteredEvent : DoorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorOpenMeteredEvent" /> class.
        /// </summary>
        public DoorOpenMeteredEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorOpenMeteredEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical door ID.</param>
        /// <param name="whilePoweredDown">Indicates if event occurred while the EGM was powered down.</param>
        /// <param name="isRecovery">Indicates whether or not this event was posted for a recovery.</param>
        /// <param name="doorName">The name of the door.</param>
        public DoorOpenMeteredEvent(int logicalId, bool whilePoweredDown, bool isRecovery, string doorName)
            : base(logicalId, whilePoweredDown, doorName)
        {
            IsRecovery = isRecovery;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not this event was posted for a recovery.
        /// </summary>
        public bool IsRecovery { get; }
    }
}