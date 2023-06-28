namespace Aristocrat.Monaco.G2S.Handlers.Voucher
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<IVoucherDevice>
    {
        private readonly ICommandBuilder<IVoucherDevice, voucherStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<IVoucherDevice, voucherStatus> commandBuilder,
            IEventLift eventLift)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public async void Handle(IVoucherDevice device)
        {
            var status = new voucherStatus();

            await _commandBuilder.Build(device, status);

            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_VCE004 : EventCode.G2S_VCE003,
                device.DeviceList(status));
        }
    }
}