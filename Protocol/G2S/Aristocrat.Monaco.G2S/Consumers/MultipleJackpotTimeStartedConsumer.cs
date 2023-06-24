namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Gaming.Contracts.Bonus;

    public class MultipleJackpotTimeStartedConsumer : Consumes<MultipleJackpotTimeStartedEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public MultipleJackpotTimeStartedConsumer(IG2SEgm egm, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(MultipleJackpotTimeStartedEvent theEvent)
        {
            var device = _egm.GetDevice<IBonusDevice>(theEvent.Transaction.DeviceId);

            _eventLift.Report(device, EventCode.IGT_BNE001, theEvent);
        }
    }
}