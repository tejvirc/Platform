namespace Aristocrat.Monaco.Hardware.Contracts.Door
{
    using System;

    /// <summary>Valid door state enumerations.</summary>
    public enum DoorState
    {
        /// <summary>Indicates door state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates door state disabled.</summary>
        Disabled,

        /// <summary>Indicates door state enabled.</summary>
        Enabled,

        /// <summary>Indicates door state error.</summary>
        Error
    }

    /// <summary>Class to be used in a generic List for management of logical doors.</summary>
    public class LogicalDoor : LogicalDeviceBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicalDoor" /> class.
        /// </summary>
        public LogicalDoor()
        {
            State = DoorState.Uninitialized;
            Closed = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicalDoor" /> class.
        /// </summary>
        /// <param name="physicalId">The physical door ID.</param>
        /// <param name="name">The door name.</param>
        /// <param name="localizedName">The localized button name.</param>
        public LogicalDoor(int physicalId, string name, string localizedName)
            : base(physicalId, name, localizedName)
        {
            State = DoorState.Uninitialized;
            Closed = true;
        }

        /// <summary>Gets or sets a value for door state.</summary>
        public DoorState State { get; set; }

        /// <summary>Gets or sets a value for door closed flag.</summary>
        public bool Closed { get; set; }

        /// <summary>Gets or sets a value for door last opened date time.</summary>
        public DateTime LastOpenedDateTime { get; set; }
    }
}