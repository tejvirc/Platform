namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<IDownloadDevice>
    {
        private readonly ICommandBuilder<IDownloadDevice, downloadStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<IDownloadDevice, downloadStatus> commandBuilder,
            IEventLift eventLift)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IDownloadDevice device)
        {
            var status = new downloadStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_DLE004 : EventCode.G2S_DLE003,
                device.DeviceList(status));
        }
    }
}