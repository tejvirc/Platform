using ProtoBuf;
using System;

namespace Aristocrat.Monaco.Kernel
{
    /// <summary>
    ///     Definition of the SystemDisableUpdatedEvent class. This is posted when a system disable has been updated.
    ///     The event contains information such as the priority, guid, and reason for the disable.
    /// </summary>
    [ProtoContract]
    public class SystemDisableUpdatedEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public SystemDisableUpdatedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDisableUpdatedEvent" /> class.
        /// </summary>
        /// <param name="priority">The priority of the disable.</param>
        /// <param name="disableId">The unique identifier for the disable.</param>
        /// <param name="reasons">The reasons, if any, for the disable.</param>
        /// <param name="systemIdleStateAffected">The flag indicating whether the system idle state is affected after adding this disable element.</param>
        public SystemDisableUpdatedEvent(
            SystemDisablePriority priority,
            Guid disableId,
            string reasons,
            bool systemIdleStateAffected)
        {
            Priority = priority;
            DisableId = disableId;
            DisableReasons = reasons;
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
        ///     Gets a flag indicating whether the system idle state is affected after adding this disable element.
        /// </summary>
        [ProtoMember(4)]
        public bool SystemIdleStateAffected { get; }
    }
}
