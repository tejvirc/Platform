namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;

    /// <summary>
    ///     Handles the <see cref="GameResultEvent" /> event.
    /// </summary>
    public class GameResultConsumer : GamePlayConsumerBase<GameResultEvent>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameResultConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance</param>
        /// <param name="gameMeters">An <see cref="IGameMeterManager" /> instance.</param>
        public GameResultConsumer(IG2SEgm egm, IEventLift eventLift, IGameMeterManager gameMeters)
            : base(egm, eventLift, gameMeters, EventCode.G2S_GPE111)
        {
        }
    }
}