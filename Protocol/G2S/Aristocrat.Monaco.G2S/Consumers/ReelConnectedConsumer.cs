﻿namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.Reel.Events;

    public class ReelConnectedConsumer : ReelBaseConsumer<ReelConnectedEvent>
    {
        public ReelConnectedConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift)
            : base(egm, commandBuilder, eventLift)
        {
        }

        public override void Consume(ReelConnectedEvent theEvent)
        {
            RemoveFault(CabinetFaults.ReelDisconnected - theEvent.ReelId);
        }
    }
}