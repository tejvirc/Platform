namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Published when a free game starts
    /// </summary>
    public class FreeGameStartedEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FreeGameStartedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        public FreeGameStartedEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log)
            : base(gameId, denomination, wagerCategory, log)
        {
        }
    }
}