namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     A StartSessionFailedEvent posted whenever it can't start a new session
    /// </summary>
    public class StartSessionFailedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StartSessionFailedEvent" /> class.
        /// </summary>
        /// <param name="readerId">The reader id identifier</param>
        public StartSessionFailedEvent(int readerId)
        {
            ReaderId = readerId;
        }

        /// <summary>
        ///     Indicate which reader Id was trying to start a new session
        /// </summary>
        public int ReaderId { get; set; }
    }
}
