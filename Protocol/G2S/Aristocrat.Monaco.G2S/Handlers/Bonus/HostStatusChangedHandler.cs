namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<IBonusDevice>
    {
        private readonly ICommandBuilder<IBonusDevice, bonusStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<IBonusDevice, bonusStatus> commandBuilder,
            IEventLift eventLift)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IBonusDevice device)
        {
            var status = new bonusStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_BNE004 : EventCode.G2S_BNE003,
                device.DeviceList(status));
        }
    }
}