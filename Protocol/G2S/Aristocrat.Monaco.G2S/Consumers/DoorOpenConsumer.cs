namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Hardware.Contracts.Door;

    /// <summary>
    ///     Handles the Hardware.Contracts.Door.ClosedEvent, which sets emits an event.
    /// </summary>
    public class DoorOpenConsumer : Consumes<OpenEvent>
    {
        private readonly IDoorService _doors;
        private readonly IG2SEgm _egm;

        public DoorOpenConsumer(IG2SEgm egm, IDoorService doors)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _doors = doors ?? throw new ArgumentNullException(nameof(doors));
        }

        /// <inheritdoc />
        public override void Consume(OpenEvent theEvent)
        {
            if(!_doors.LogicalDoors.ContainsKey(theEvent.LogicalId))
            {
                return;
            }

            if (!theEvent.WhilePoweredDown || !_doors.GetDoorClosed(theEvent.LogicalId))
            {
                var device = _egm.GetDevice<ICabinetDevice>();

                device?.AddCondition(device, EgmState.EgmDisabled, theEvent.LogicalId);
            }
        }
    }
}
