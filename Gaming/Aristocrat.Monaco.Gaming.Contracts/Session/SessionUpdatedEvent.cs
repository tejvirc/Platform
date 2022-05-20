namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    /// <summary>
    ///     The Session Updated Event is published at the end of every game cycle
    /// </summary>
    public class SessionUpdatedEvent : BaseSessionEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdatedEvent" /> class.
        /// </summary>
        /// <param name="log">The player session info</param>
        public SessionUpdatedEvent(IPlayerSessionLog log)
            : base(log)
        {
        }
    }
}
