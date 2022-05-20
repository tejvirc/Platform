namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     A GameResultEvent should be posted by the game immediately upon determining the outcome of a game play, and
    ///     before presenting the results. A GameResultEvent should only be posted once per game play.
    /// </summary>
    public class GameResultEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameResultEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        public GameResultEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log)
            : base(gameId, denomination, wagerCategory, log)
        {
        }
    }
}