namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.Reel.Events;

    public class ReelFaultClearConsumer : ReelBaseConsumer<HardwareReelFaultClearEvent>
    {
        public ReelFaultClearConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift)
            : base(egm, commandBuilder, eventLift)
        {
        }

        public override void Consume(HardwareReelFaultClearEvent theEvent)
        {
            RemoveFault((int)CabinetFaults.ReelFault - (int)theEvent.Fault);
        }
    }
}