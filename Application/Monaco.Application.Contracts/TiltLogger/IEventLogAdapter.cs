namespace Aristocrat.Monaco.Application.Contracts.TiltLogger
{
    using System.Collections.Generic;

    /// <summary>
    ///     Interface to extract logs from event information.
    /// </summary>
    public interface IEventLogAdapter
    {
        /// <summary>
        ///     Gets event logs
        /// </summary>
        /// <returns>A list of event longs for particular kind of events</returns>
        IEnumerable<EventDescription> GetEventLogs();

        /// <summary>
        ///     Gets maximum log sequence value
        /// </summary>
        /// <returns> Max log sequence value</returns>
        long GetMaxLogSequence();

        /// <summary>
        /// </summary>
        string LogType { get; }
    }
}
