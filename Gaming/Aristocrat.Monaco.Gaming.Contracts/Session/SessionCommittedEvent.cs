namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using Kernel;

    /// <summary>
    ///     The Session Committed Event is published when a player session is completed
    /// </summary>
    public class SessionCommittedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionCommittedEvent" /> class.
        /// </summary>
        /// <param name="log">The player session info</param>
        public SessionCommittedEvent(IPlayerSessionLog log)
        {
            Log = log;
        }

        /// <summary>
        ///     Gets the player session associated with the event
        /// </summary>
        public IPlayerSessionLog Log { get; }
    }
}
