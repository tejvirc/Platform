using Aristocrat.Monaco.Hardware.Contracts.Door;

namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    /// <summary>
    /// Enum for the two types of Technician Mode Triggers
    /// Door and Button
    /// Door will toggle on open and shut
    /// Button will toggle on every button press (up) event.
    /// </summary>
    public enum TechnicianModeTriggerType
    {
        /// <summary>
        /// Door Trigger Type
        /// </summary>
        Door,
        /// <summary>
        /// Button Trigger Type
        /// </summary>
        Button
    }

    /// <summary>
    /// Data class for OperatorMenutechincianModeTrigger
    /// </summary>
    public class OperatorMenuTechnicianModeTrigger
    {
        /// <summary>
        /// Constructor for OperatorMenuTechnicianModeTrigger
        /// </summary>
        /// <param name="triggerType"></param>
        /// <param name="logicalId"></param>
        /// <param name="requiresMainDoorOpen"></param>
        public OperatorMenuTechnicianModeTrigger(TechnicianModeTriggerType triggerType = TechnicianModeTriggerType.Door, int logicalId = (int)DoorLogicalId.Logic, bool requiresMainDoorOpen = true)
        {
            TriggerType = triggerType;
            LogicalId = logicalId;
            RequiresMainDoorOpen = requiresMainDoorOpen;
        }

        /// <summary>
        /// TriggerType Property
        /// </summary>
        public TechnicianModeTriggerType TriggerType { get; }

        /// <summary>
        /// LogicalId Property
        /// </summary>
        public int LogicalId { get; }

        /// <summary>
        /// RequiresMainDoorOpen Property
        /// </summary>
        public bool RequiresMainDoorOpen { get; }
    }
}
