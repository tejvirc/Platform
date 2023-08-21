namespace Aristocrat.Monaco.Application
{
    using Contracts;
    using Hardware.Contracts.Door;
    using Kernel;

    /// <summary>
    ///     A rule that blocks clearing of persisted data unless the logic door is open.
    /// </summary>
    public class LogicDoorOpenPersistenceClearRule : BasePersistenceClearRule
    {
        private const string DenyReason = "The Logic Door must be open.";

        /// <summary>Initializes a new instance of the <see cref="LogicDoorOpenPersistenceClearRule" /> class.</summary>
        public LogicDoorOpenPersistenceClearRule()
        {
            var serviceManager = ServiceManager.GetInstance();

            var door = serviceManager.GetService<IDoorService>();
            var allow = door.GetDoorClosed((int)DoorLogicalId.Logic) == false;
            SetAllowed(allow, allow, DenyReason);

            var eventBus = serviceManager.GetService<IEventBus>();
            eventBus.Subscribe<OpenEvent>(this, HandleDoorOpenEvent);
            eventBus.Subscribe<ClosedEvent>(this, HandleDoorClosedEvent);
        }

        private void HandleDoorOpenEvent(IEvent theEvent)
        {
            if (((OpenEvent)theEvent).LogicalId == (int)DoorLogicalId.Logic)
            {
                SetAllowed(true, true, DenyReason);
            }
        }

        private void HandleDoorClosedEvent(IEvent theEvent)
        {
            if (((ClosedEvent)theEvent).LogicalId == (int)DoorLogicalId.Logic)
            {
                SetAllowed(false, false, DenyReason);
            }
        }
    }
}