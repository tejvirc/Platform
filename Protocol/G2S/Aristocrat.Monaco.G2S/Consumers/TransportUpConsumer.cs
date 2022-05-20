namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Common.Events;

    public class TransportUpConsumer : Consumes<TransportUpEvent>
    {
        private readonly IG2SEgm _egm;

        public TransportUpConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        public override void Consume(TransportUpEvent theEvent)
        {
            var comms = _egm.GetDevice<ICommunicationsDevice>(theEvent.HostId);

            if (comms != null)
            {
                _egm.GetDevice<ICabinetDevice>()?.RemoveCondition(comms, EgmState.TransportDisabled);
            }
        }
    }
}