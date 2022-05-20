namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.Persistence;

    public class StorageErrorConsumer : Consumes<StorageErrorEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public StorageErrorConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(StorageErrorEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }

            if (device.HasCondition(
                (d, s, f) => d is ICabinetDevice && s == EgmState.EgmDisabled && f == (int)CabinetFaults.StorageFault))
            {
                return;
            }

            device.AddCondition(device, EgmState.EgmDisabled, (int)CabinetFaults.StorageFault);

            var status = new cabinetStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(device, EventCode.G2S_CBE311, device.DeviceList(status));
        }
    }
}