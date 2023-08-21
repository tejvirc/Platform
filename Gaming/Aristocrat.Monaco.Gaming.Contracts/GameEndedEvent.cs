namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     A GameEndedEvent should be posted when the entire game is over, the final win amount has been determined, and the
    ///     game transaction will be terminated.  A GameEndedEvent should only be posted once per game play.
    /// </summary>
    public class GameEndedEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEndedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        public GameEndedEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log)
            : base(gameId, denomination, wagerCategory, log)
        {
        }
    }
}