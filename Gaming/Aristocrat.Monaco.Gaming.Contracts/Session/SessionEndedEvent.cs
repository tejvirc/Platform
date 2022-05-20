namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    /// <summary>
    ///     The Session Ended Event is published when a player session ends
    /// </summary>
    public class SessionEndedEvent : BaseSessionEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionEndedEvent" /> class.
        /// </summary>
        /// <param name="log">The player session info</param>
        public SessionEndedEvent(IPlayerSessionLog log)
            : base(log)
        {
        }
    }
}
