namespace Aristocrat.Monaco.G2S.Handlers.Handpay
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<IHandpayDevice>
    {
        private readonly ICommandBuilder<IHandpayDevice, handpayStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<IHandpayDevice, handpayStatus> commandBuilder,
            IEventLift eventLift)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IHandpayDevice device)
        {
            var status = new handpayStatus();

            _commandBuilder.Build(device, status);

            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_JPE004 : EventCode.G2S_JPE003,
                device.DeviceList(status));
        }
    }
}