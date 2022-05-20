namespace Aristocrat.Monaco.G2S.Handlers.InformedPlayer
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<IInformedPlayerDevice>
    {
        private readonly ICommandBuilder<IInformedPlayerDevice, ipStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<IInformedPlayerDevice, ipStatus> commandBuilder,
            IEventLift eventLift)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IInformedPlayerDevice device)
        {
            var status = new ipStatus();

            _commandBuilder.Build(device, status);

            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_IPE004 : EventCode.G2S_IPE003,
                device.DeviceList(status));
        }
    }
}