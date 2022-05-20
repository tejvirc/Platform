namespace Aristocrat.Monaco.G2S.Handlers.Spc
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<ISpcDevice>
    {
        private readonly ICommandBuilder<ISpcDevice, spcStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<ISpcDevice, spcStatus> commandBuilder,
            IEventLift eventLift)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(ISpcDevice device)
        {
            var status = new spcStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_SPE004 : EventCode.G2S_SPE003,
                device.DeviceList(status));
        }
    }
}
