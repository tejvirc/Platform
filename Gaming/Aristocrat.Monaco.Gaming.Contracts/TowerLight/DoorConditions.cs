namespace Aristocrat.Monaco.Gaming.Contracts.TowerLight
{
    using System;

    /// <summary>
    ///     Door condition to determine TowerLight's flash status (Note: Some of the conditions are NOT mutually
    ///     exclusive)
    /// </summary>
    [Flags]
    public enum DoorConditions
    {
        /// <summary>None</summary>
        None = 0,

        /// <summary>All doors are closed</summary>
        AllClosed = 1,

        /// <summary>Indicates any of the door was once open (This condition resets when a new game round starts)</summary>
        DoorWasOpenBefore = 1 << 1,

        /// <summary>Main door is open</summary>
        MainDoorOpen = 1 << 2,

        /// <summary>Drop door is open</summary>
        DropDoorOpen = 1 << 3,

        /// <summary>Any of the door is currently open</summary>
        DoorOpen = 1 << 4,

        /// <summary>Indicates any of the door was once open (This condition resets when a new game round ends)</summary>
        DoorWasOpenBeforeResetEndGame = 1 << 5,
    }
}