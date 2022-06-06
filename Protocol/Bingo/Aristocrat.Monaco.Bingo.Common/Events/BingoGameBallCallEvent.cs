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
        /// <param name="daubs">the card daubs</param>
        /// <param name="isRecovery">Whether or not this is for recovery</param>
        public BingoGameBallCallEvent(BingoBallCall ballCall, int daubs, bool isRecovery = false)
        {
            BallCall = ballCall;
            Daubs = daubs;
            IsRecovery = isRecovery;
        }

        /// <summary>
        ///     Get the <see cref="BingoBallCall"/>.
        /// </summary>
        public BingoBallCall BallCall { get; }

        /// <summary>
        ///     Get the bingo card daubs packed into an int
        /// </summary>
        public int Daubs { get; }

        /// <summary>
        ///     Gets whether or not the ball call is for recovery
        /// </summary>
        public bool IsRecovery { get; }
    }
}
