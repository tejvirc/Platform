namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Indicates the secondary game has ended.  This event may be posted multiple times during a game round.
    /// </summary>
    public class SecondaryGameEndedEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SecondaryGameEndedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        /// <param name="win">The total win amount for the primary game.</param>
        public SecondaryGameEndedEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log, long win)
            : base(gameId, denomination, wagerCategory, log)
        {
            Win = win;
        }

        /// <summary>
        ///     Gets the total win amount.
        /// </summary>
        public long Win { get; }
    }
}