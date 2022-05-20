namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.Audio;

    public class AudioDisconnectedConsumer : Consumes<DisabledEvent>
    {
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public AudioDisconnectedConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(DisabledEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }

            device.AddCondition(device, EgmState.EgmDisabled, (int)CabinetFaults.AudioDisconnected);

            var status = new cabinetStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(device, EventCode.G2S_CBE309, device.DeviceList(status));
        }
    }
}