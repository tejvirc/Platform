namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Gaming.Contracts.Session;
    using Handlers.Player;

    public class PlayerSessionAcknowledgedConsumer : Consumes<SessionCommittedEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public PlayerSessionAcknowledgedConsumer(IG2SEgm egm, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(SessionCommittedEvent theEvent)
        {
            var device = _egm.GetDevice<IPlayerDevice>();
            if (device == null)
            {
                return;
            }

            _eventLift.Report(
                device,
                EventCode.G2S_PRE104,
                theEvent.Log.TransactionId,
                device.TransactionList(theEvent.Log.ToPlayerLog(device)));
        }
    }
}
