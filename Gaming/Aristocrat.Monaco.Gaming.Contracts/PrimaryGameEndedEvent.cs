namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Indicates the primary game has ended.  This event may be posted multiple times during a game round.
    /// </summary>
    public class PrimaryGameEndedEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryGameEndedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        /// <param name="initialWin">The initial win from primary game, including progressive win amounts.</param>
        public PrimaryGameEndedEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log, long initialWin)
            : base(gameId, denomination, wagerCategory, log)
        {
            InitialWin = initialWin;
        }

        /// <summary>
        ///     Gets the initial win from primary game, including progressive win amounts
        /// </summary>
        public long InitialWin { get; }
    }
}