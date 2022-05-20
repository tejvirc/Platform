namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;

    public class SecondaryGameEscrowConsumer : GamePlayConsumerBase<SecondaryGameEscrowEvent>
    {
        public SecondaryGameEscrowConsumer(IG2SEgm egm, IEventLift eventLift, IGameMeterManager gameMeters)
            : base(egm, eventLift, gameMeters, EventCode.G2S_GPE107)
        {
        }
    }
}