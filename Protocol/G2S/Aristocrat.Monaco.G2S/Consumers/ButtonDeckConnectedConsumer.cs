namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Hardware.Contracts.ButtonDeck;

    public class ButtonDeckConnectedConsumer : Consumes<ButtonDeckConnectedEvent>
    {
        private readonly IG2SEgm _egm;

        public ButtonDeckConnectedConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        public override void Consume(ButtonDeckConnectedEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();

            device?.RemoveCondition(device, EgmState.EgmDisabled, (int)CabinetFaults.ButtonDeckDisconnected);
        }
    }
}