namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Hardware.Contracts.Display;

    public class DisplayConnectedConsumer : Consumes<DisplayConnectedEvent>
    {
        private readonly IG2SEgm _egm;

        public DisplayConnectedConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        public override void Consume(DisplayConnectedEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();

            device?.RemoveCondition(device, EgmState.EgmDisabled, (int)CabinetFaults.DisplayDisconnected);
        }
    }
}