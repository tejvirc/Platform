namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Common.Events;

    public class HostUnreachableConsumer : Consumes<HostUnreachableEvent>
    {
        private readonly IG2SEgm _egm;

        public HostUnreachableConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        public override void Consume(HostUnreachableEvent theEvent)
        {
            var comms = _egm.GetDevice<ICommunicationsDevice>(theEvent.HostId);

            if (comms != null)
            {
                _egm.GetDevice<ICabinetDevice>()?.AddCondition(comms, EgmState.TransportDisabled);
            }
        }
    }
}