namespace Aristocrat.Monaco.Kernel
{
    using ProtoBuf;
    using System;

    /// <summary>
    ///     Definition of the SystemDisableRemovedEvent class. This is posted when a currently active system disable is removed
    ///     from the system. This event includes information such as the priority, the Guid, and the disable reason.
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class SystemDisableRemovedEvent : BaseEvent
    {

        /// <summary>
        /// Empty Constructor for deserialization
        /// </summary>
        public SystemDisableRemovedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDisableRemovedEvent" /> class.
        /// </summary>
        /// <param name="priority">The priority of the disable.</param>
        /// <param name="disableId">The unique identifier for the disable.</param>
        /// <param name="reasons">The reasons, if any, for the disable.</param>
        /// <param name="systemDisabled">The value indicating whether the system is still disabled or not after removing this disable element.</param>
        /// <param name="systemIdleStateAffected">The flag indicating whether the system idle state is still affected after removing this disable element.</param>
        public SystemDisableRemovedEvent(
            SystemDisablePriority priority,
            Guid disableId,
            string reasons,
            bool systemDisabled,
            bool systemIdleStateAffected)
        {
            Priority = priority;
            DisableId = disableId;
            DisableReasons = reasons;
            SystemDisabled = systemDisabled;
            SystemIdleStateAffected = systemIdleStateAffected;
        }

        /// <summary>
        ///     Gets the priority of the disable.
        /// </summary>
        [ProtoMember(1)]
        public SystemDisablePriority Priority { get; }

        /// <summary>
        ///     Gets the Globally Unique ID for the disable.
        /// </summary>
        [ProtoMember(2)]
        public Guid DisableId { get; }

        /// <summary>
        ///     Gets the reason
        /// </summary>
        [ProtoMember(3)]
        public string DisableReasons { get; }

        /// <summary>
        ///     Gets a value indicating whether the system is still disabled or not after removing this disable element.
        /// </summary>
        [ProtoMember(4)]
        public bool SystemDisabled { get; }

        /// <summary>
        ///     Gets a flag indicating whether the system idle state is still affected after removing this disable element.
        /// </summary>
        [ProtoMember(5)]
        public bool SystemIdleStateAffected { get; }
    }
}
