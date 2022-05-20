namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using GameOverlay;
    using Kernel;

    /// <summary>
    ///     BingoGame ball call event.
    /// </summary>
    public class BingoGameBallCallEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor for <see cref="BingoGameBallCallEvent"/>
        /// </summary>
        /// <param name="ballCall">A <see cref="BingoBallCall"/>.</param>
        public BingoGameBallCallEvent(BingoBallCall ballCall)
        {
            BallCall = ballCall;
        }

        /// <summary>
        ///     Constructor for <see cref="BingoGameBallCallEvent"/>
        /// </summary>
        /// <param name="ballCall">A <see cref="BingoBallCall"/>.</param>
        /// <param name="daubs">the card daubs</param>
        public BingoGameBallCallEvent(BingoBallCall ballCall, int daubs)
        {
            BallCall = ballCall;
            Daubs = daubs;
        }

        /// <summary>
        ///     Get the <see cref="BingoBallCall"/>.
        /// </summary>
        public BingoBallCall BallCall { get; }

        /// <summary>
        ///     Get the bingo card daubs packed into an int
        /// </summary>
        public int Daubs { get; }
    }
}
