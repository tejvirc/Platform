namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     The SecondaryGameStarted event indicates that a secondary game cycle has been initiated by the player and the
    ///     secondary wager has been committed to the accounting meters.  This event may be posted zero to many times in a game
    ///     round.
    /// </summary>
    public class SecondaryGameStartedEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SecondaryGameStartedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        /// <param name="betAmount">The total bet amount for the primary game.</param>
        public SecondaryGameStartedEvent(
            int gameId,
            long denomination,
            string wagerCategory,
            IGameHistoryLog log,
            long betAmount)
            : base(gameId, denomination, wagerCategory, log)
        {
            BetAmount = betAmount;
        }

        /// <summary>
        ///     Gets the total bet amount.
        /// </summary>
        public long BetAmount { get; }
    }
}