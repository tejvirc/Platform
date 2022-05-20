namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Handlers.Central;

    public class OutcomeRequestedConsumer : Consumes<OutcomeRequestedEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IGameProvider _games;

        public OutcomeRequestedConsumer(IG2SEgm egm, IEventLift eventLift, IGameProvider games)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _games = games ?? throw new ArgumentNullException(nameof(games));
        }

        public override void Consume(OutcomeRequestedEvent theEvent)
        {
            var device = _egm.GetDevice<ICentralDevice>(theEvent.Transaction.DeviceId);
            if (device == null)
            {
                return;
            }

            _eventLift.Report(
                device,
                EventCode.G2S_CLE101,
                theEvent.Transaction.TransactionId,
                device.TransactionList(theEvent.Transaction.ToCentralLog(_games)));
        }
    }
}