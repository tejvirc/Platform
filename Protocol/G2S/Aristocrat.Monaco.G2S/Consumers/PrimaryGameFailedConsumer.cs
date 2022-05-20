namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;

    public class PrimaryGameFailedConsumer : GamePlayConsumerBase<PrimaryGameFailedEvent>
    {
        public PrimaryGameFailedConsumer(IG2SEgm egm, IEventLift eventLift, IGameMeterManager gameMeters)
            : base(egm, eventLift, gameMeters, EventCode.G2S_GPE102)
        {
        }
    }
}