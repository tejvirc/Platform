namespace Aristocrat.Monaco.G2S.Handlers.CoinAcceptor
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<ICoinAcceptor>
    {
        private readonly ICommandBuilder<ICoinAcceptor, coinAcceptorStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<ICoinAcceptor, coinAcceptorStatus> commandBuilder,
            IEventLift eventLift)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(ICoinAcceptor device)
        {
            var status = new coinAcceptorStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_CAE004 : EventCode.G2S_CAE003,
                device.DeviceList(status));
        }
    }
}