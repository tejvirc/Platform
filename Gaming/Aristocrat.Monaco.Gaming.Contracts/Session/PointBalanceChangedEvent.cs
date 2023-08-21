namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    /// <summary>
    ///     The Point Balance Changed Event is published when the player's points for the session is accumulated
    /// </summary>
    public class PointBalanceChangedEvent : BaseSessionEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PointBalanceChangedEvent" /> class.
        /// </summary>
        /// <param name="log">The player session info</param>
        public PointBalanceChangedEvent(IPlayerSessionLog log)
            : base(log)
        {
        }
    }
}
