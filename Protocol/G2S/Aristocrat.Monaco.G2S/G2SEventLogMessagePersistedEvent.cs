namespace Aristocrat.Monaco.G2S
{
    using Aristocrat.Monaco.G2S.Common.G2SEventLogger;
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    /// Class which encapsulates a G2SEventLogMessage after it gets persisted.
    /// </summary>
    public class G2SEventLogMessagePersistedEvent : BaseEvent
    {
        /// <summary>
        /// The persisted event log message.
        /// </summary>
        public G2SEventLogMessage EventLog { get; }

        /// <summary>
        /// Creates an event which encapsulates a G2SEventLogMessage after it gets persisted.
        /// </summary>
        /// <param name="eventLog"></param>
        public G2SEventLogMessagePersistedEvent(G2SEventLogMessage eventLog)
        {
            EventLog = eventLog;
        }
    }
}