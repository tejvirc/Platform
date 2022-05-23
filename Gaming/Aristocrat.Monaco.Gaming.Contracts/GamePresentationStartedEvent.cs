namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Published when a game (base game or free game) outcome begins its presentation to the player
    /// </summary>
    public class GamePresentationStartedEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePresentationStartedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        public GamePresentationStartedEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log)
            : base(gameId, denomination, wagerCategory, log)
        {
        }
    }
}