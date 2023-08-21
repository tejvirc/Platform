namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     The PrimaryGameEscrowEvent event is generated when entering the primary game escrow state.  This will only apply
    ///     when a central determinant game is being played.
    /// </summary>
    public class PrimaryGameEscrowEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryGameEscrowEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        public PrimaryGameEscrowEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log)
            : base(gameId, denomination, wagerCategory, log)
        {
        }
    }
}