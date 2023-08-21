namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    /// <summary>
    ///     The Session Started Event is published when a valid player identification has been presented
    /// </summary>
    public class SessionStartedEvent : BaseSessionEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionStartedEvent" /> class.
        /// </summary>
        /// <param name="log">The player session info</param>
        public SessionStartedEvent(IPlayerSessionLog log)
            : base(log)
        {
        }
    }
}
