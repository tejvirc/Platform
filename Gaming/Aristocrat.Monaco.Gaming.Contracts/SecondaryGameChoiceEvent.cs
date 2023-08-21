namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     The SecondaryGameChoiceEvent event indicates the game has entered a secondary game choice.
    /// </summary>
    public class SecondaryGameChoiceEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SecondaryGameChoiceEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        public SecondaryGameChoiceEvent(
            int gameId,
            long denomination,
            string wagerCategory,
            IGameHistoryLog log)
            : base(gameId, denomination, wagerCategory, log)
        {
        }
    }
}