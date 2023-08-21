namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using Kernel;

    /// <summary>
    ///     Abstract class for session related events
    /// </summary>
    public abstract class BaseSessionEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseSessionEvent" /> class.
        /// </summary>
        /// <param name="log">The player session info</param>
        protected BaseSessionEvent(IPlayerSessionLog log)
        {
            Log = log.ShallowCopy();
        }

        /// <summary>
        ///     Gets the player session log associated with the event
        /// </summary>
        public IPlayerSessionLog Log { get; }
    }
}
