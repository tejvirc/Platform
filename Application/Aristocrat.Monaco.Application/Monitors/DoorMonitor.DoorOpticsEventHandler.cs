namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;

    public partial class DoorMonitor
    {
        private class DoorOpticsEventHandler
        {
            private readonly int _physicalDoorId;
            private readonly IDictionary<int, int> _doorPair = new Dictionary<int, int>();
            private readonly ISystemDisableManager _disableManager;
            private readonly IMessageDisplay _messageDisplay;

            public DoorOpticsEventHandler(
                int physicalDoorId, int opticDoorId,
                ISystemDisableManager disableManager,
                IMessageDisplay messageDisplay)
            {
                _physicalDoorId = physicalDoorId;
                _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
                _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));

                // Prepare a bidirectional lookup for both doors.
                _doorPair.Add(physicalDoorId, opticDoorId);
                _doorPair.Add(opticDoorId, physicalDoorId);
            }

            public void OpenDoor(int logicalId, DoorMonitor doorMonitor)
            {
                if (!_doorPair.TryGetValue(logicalId, out var pairDoorId))
                {
                    return;
                }

                var door = (DoorInfoWithMismatch)doorMonitor._doors[_physicalDoorId];

                // Remove any message saying "XYZ door closed"
                _messageDisplay.RemoveMessage(door.GetDoorClosedMessage(CultureProviderType.Player));

                // Check if the paired door is closed, display mismatch error if it is
                if (!doorMonitor.IsDoorOpen(pairDoorId))
                {
                    _disableManager.Disable(
                        door.DoorGuid,
                        SystemDisablePriority.Immediate,
                        door.DoorMismatchMessage.MessageResourceKey,
                        door.DoorMismatchMessage.CultureProvider);
                }
                else
                {
                    var doorOpenMessage = door.GetDoorOpenMessage();
                    // Otherwise we can just now say that the whole door is open
                    _disableManager.Disable(
                        door.DoorGuid,
                        SystemDisablePriority.Immediate,
                        doorOpenMessage.MessageResourceKey,
                        doorOpenMessage.CultureProvider);
                }
            }

            public void CloseDoor(int logicalId, DoorMonitor doorMonitor)
            {
                if (!_doorPair.TryGetValue(logicalId, out var pairDoorId))
                {
                    return;
                }

                var door = (DoorInfoWithMismatch)doorMonitor._doors[_physicalDoorId];

                // Check if the paired door is closed, display mismatch error if it is
                if (doorMonitor.IsDoorOpen(pairDoorId))
                {
                    _disableManager.Disable(
                        door.DoorGuid,
                        SystemDisablePriority.Immediate,
                        door.DoorMismatchMessage.MessageResourceKey,
                        door.DoorMismatchMessage.CultureProvider);
                }
                else
                {
                    // Otherwise we can just now say that the whole door is closed
                    _disableManager.Enable(door.DoorGuid);

                    // Create a message saying "XYZ door closed"
                    _messageDisplay.DisplayMessage(door.GetDoorClosedMessage());
                }
            }
        }
    }
}