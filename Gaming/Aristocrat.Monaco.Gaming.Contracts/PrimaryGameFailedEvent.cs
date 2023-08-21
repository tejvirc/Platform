namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     The PrimaryGameFailedEvent event is generated when the central determinant game has failed to provide a valid
    ///     outcome.
    /// </summary>
    public class PrimaryGameFailedEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryGameFailedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        public PrimaryGameFailedEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log)
            : base(gameId, denomination, wagerCategory, log)
        {
        }
    }
}