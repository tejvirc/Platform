namespace Aristocrat.Monaco.G2S.Handlers.Central
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<ICentralDevice>
    {
        private readonly ICommandBuilder<ICentralDevice, centralStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<ICentralDevice, centralStatus> commandBuilder,
            IEventLift eventLift)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(ICentralDevice device)
        {
            var status = new centralStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_CLE004 : EventCode.G2S_CLE003,
                device.DeviceList(status));
        }
    }
}