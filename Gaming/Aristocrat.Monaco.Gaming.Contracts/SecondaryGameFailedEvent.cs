namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     The SecondaryGameFailedEvent event is generated when the central determinant game has failed to provide a valid
    ///     outcome.
    /// </summary>
    public class SecondaryGameFailedEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SecondaryGameFailedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        public SecondaryGameFailedEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log)
            : base(gameId, denomination, wagerCategory, log)
        {
        }
    }
}