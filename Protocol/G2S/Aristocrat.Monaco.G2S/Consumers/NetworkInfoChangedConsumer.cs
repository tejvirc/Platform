namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Common.Events;
    using Kernel;

    public class NetworkInfoChangedConsumer : Consumes<NetworkInfoChangedEvent>
    {
        private readonly IEventBus _bus;
        private readonly IG2SEgm _egm;

        public NetworkInfoChangedConsumer(IG2SEgm egm, IEventBus bus)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public override void Consume(NetworkInfoChangedEvent theEvent)
        {
            if (_egm != null &&
                !_egm.Address.Host.Equals(theEvent.NetworkInfo.IpAddress, StringComparison.InvariantCultureIgnoreCase))
            {
                _bus.Publish(new RestartProtocolEvent());
            }
        }
    }
}