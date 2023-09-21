namespace Aristocrat.Monaco.G2S.Handlers.Analytics
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<IAnalyticsDevice>
    {
        private readonly ICommandBuilder<IAnalyticsDevice, analyticsStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<IAnalyticsDevice, analyticsStatus> commandBuilder,
            IEventLift eventLift)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IAnalyticsDevice device)
        {
            var status = new analyticsStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.ATI_ANE004 : EventCode.ATI_ANE003,
                device.DeviceList(status));
        }
    }
}