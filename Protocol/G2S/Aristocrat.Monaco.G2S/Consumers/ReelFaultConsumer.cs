namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.Reel.Events;

    public class ReelFaultConsumer : ReelBaseConsumer<HardwareReelFaultEvent>
    {
        public ReelFaultConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift)
            : base(egm, commandBuilder, eventLift)
        {
        }

        public override void Consume(HardwareReelFaultEvent theEvent)
        {
            AddFault((int)CabinetFaults.ReelFault - (int)theEvent.Fault);
        }
    }
}