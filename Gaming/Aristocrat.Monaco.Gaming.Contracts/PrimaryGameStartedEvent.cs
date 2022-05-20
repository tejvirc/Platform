namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     The PrimaryGameStarted event indicates that a primary game-cycle has been started by the player and the initial
    ///     wager has been committed to the accounting meters. This state also indicates that the ‘primary-game-cycle’ can not
    ///     be aborted. Some game types may permit additional wagers (blackjack double-down, insurance, split, etc.) while in
    ///     the primary game cycle, therefore additional wager changes may be committed to the accounting meters while in this
    ///     state.
    /// </summary>
    public class PrimaryGameStartedEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryGameStartedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        public PrimaryGameStartedEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log)
            : base(gameId, denomination, wagerCategory, log)
        {
        }
    }
}