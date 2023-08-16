namespace Aristocrat.Monaco.Hardware.Contracts.Door
{
    using System;
    using Kernel;
    using Properties;
    using ProtoBuf;

    /// <summary>Class to handle door specific events.</summary>
    [ProtoContract]
    public abstract class DoorBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorBaseEvent" /> class.
        /// </summary>
        protected DoorBaseEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorBaseEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical ID of the door.</param>
        /// <param name="whilePoweredDown">Indicates if event occurred while the EGM was powered down.</param>
        /// <param name="doorName">The name of the door.</param>
        protected DoorBaseEvent(int logicalId, bool whilePoweredDown, string doorName)
        {
            LogicalId = logicalId;
            WhilePoweredDown = whilePoweredDown;
            DoorName = doorName;
        }

        /// <summary>Gets the value of the LogicalId.</summary>
        [ProtoMember(1)]
        public int LogicalId { get; }

        /// <summary>Gets a value indicating whether the event occurred while the EGM was powered down.</summary>
        [ProtoMember(2)]
        public bool WhilePoweredDown { get; }

        /// <summary>Gets the value of the door name.</summary>
        [ProtoMember(3)]
        public string DoorName { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return WhilePoweredDown
                ? $"{Resources.PowerOffText} {DoorName} {GetType().Name}"
                : $"{DoorName} {GetType().Name}";
        }

        /// <summary>Assembles and returns a localized string</summary>
        /// <param name="localizedDoorName"></param>
        /// <returns>A string representation of the event and its data</returns>
        public string ToLocalizedString(string localizedDoorName)
        {
            return WhilePoweredDown
                ? $"{Resources.PowerOffText} {localizedDoorName} {GetType().Name}"
                : $"{localizedDoorName} {GetType().Name}";
        }
    }
}