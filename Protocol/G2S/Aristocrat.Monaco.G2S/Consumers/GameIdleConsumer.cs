namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;

    /// <summary>
    ///     Handles the <see cref="GameIdleEvent" /> event.
    /// </summary>
    public class GameIdleConsumer : GamePlayConsumerBase<GameIdleEvent>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameIdleConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance</param>
        /// <param name="gameMeters">An <see cref="IGameMeterManager" /> instance.</param>
        public GameIdleConsumer(IG2SEgm egm, IEventLift eventLift, IGameMeterManager gameMeters)
            : base(egm, eventLift, gameMeters, EventCode.G2S_GPE113)
        {
        }
    }
}