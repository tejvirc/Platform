namespace Aristocrat.Monaco.Hardware.Contracts.Door
{
    using System;
    using System.Collections.Generic;

    /// <summary>Logical Door state enumerations.</summary>
    public enum DoorLogicalState
    {
        /// <summary>Indicates door state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates door state idle.</summary>
        Idle,

        /// <summary>Indicates door state error.</summary>
        Error,

        /// <summary>Indicates door state disabled.</summary>
        Disabled
    }

    /// <summary>Definition of the IDoor interface.</summary>
    public interface IDoorService
    {
        /// <summary>Gets the dictionary of logical doors.</summary>
        /// <returns>Dictionary of logical doors.</returns>
        IReadOnlyDictionary<int, LogicalDoor> LogicalDoors { get; }

        /// <summary>Gets the list of ignored doors.</summary>
        /// <returns>list of ignored doors.</returns>
        List<int> IgnoredDoors { get; set; }

        /// <summary>Gets the logical door service state.</summary>
        /// <returns>The logical door service state.</returns>
        DoorLogicalState LogicalState { get; }

        /// <summary>Gets door closed flag value for the given logical door ID.</summary>
        /// <param name="doorId">The logical door ID.</param>
        /// <returns>bool:  true is closed</returns>
        bool GetDoorClosed(int doorId);

        // <summary>Gets door open value for the given logical door ID.</summary>
        /// <param name="doorId">The logical door ID.</param>
        /// <returns>bool:  true is opened</returns>
        bool GetDoorOpen(int doorId);

        /// <summary>Gets the door name for the given logical door ID.</summary>
        /// <param name="doorId">The logical door ID.</param>
        /// <returns>The door name.</returns>
        string GetDoorName(int doorId);

        /// <summary>Gets the door last opened date time for the given logical door ID.</summary>
        /// <param name="doorId">The logical door ID.</param>
        /// <returns>The date time the door was last opened.</returns>
        DateTime GetDoorLastOpened(int doorId);

        /// <summary>Gets the physical door ID for the given logical door ID.</summary>
        /// <param name="doorId">The logical door ID.</param>
        /// <returns>The physical door ID.</returns>
        int GetDoorPhysicalId(int doorId);

        /// <summary>Gets the door state enumeration value for the given logical door ID.</summary>
        /// <param name="doorId">The logical door ID.</param>
        /// <returns>An enumerated door state.</returns>
        DoorState GetDoorState(int doorId);

    }
}