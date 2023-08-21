namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Bonus;
    using Handlers;

    public class GameDelayPeriodEndedConsumer : Consumes<GameDelayPeriodEndedEvent>
    {
        private readonly ICommandBuilder<IBonusDevice, bonusStatus> _deviceStatus;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public GameDelayPeriodEndedConsumer(
            IG2SEgm egm,
            ICommandBuilder<IBonusDevice, bonusStatus> deviceStatus,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _deviceStatus = deviceStatus ?? throw new ArgumentNullException(nameof(deviceStatus));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(GameDelayPeriodEndedEvent theEvent)
        {
            var device = _egm.GetDevice<IBonusDevice>(theEvent.DeviceId);

            var bonusStatus = new bonusStatus();
            _deviceStatus.Build(device, bonusStatus);

            _eventLift.Report(
                device,
                EventCode.G2S_BNE102,
                device.DeviceList(bonusStatus),
                theEvent);
        }
    }
}