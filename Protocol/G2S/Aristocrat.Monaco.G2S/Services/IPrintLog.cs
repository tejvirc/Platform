namespace Aristocrat.Monaco.G2S.Services
{
    using System.Collections.Generic;
    using Data.Model;

    /// <summary>
    ///     Provides a mechanism for interacting with the print log
    /// </summary>
    public interface IPrintLog
    {
        /// <summary>
        ///     Gets the minimum number of log entries supported
        /// </summary>
        int MinimumLogEntries { get; }

        /// <summary>
        ///     Gets the total number of log entries
        /// </summary>
        int Entries { get; }

        /// <summary>
        ///     Gets the total number of log entries
        /// </summary>
        long LastSequence { get; }

        /// <summary>
        ///     Gets the logs
        /// </summary>
        /// <returns>A collection of <see cref="PrintLog" /></returns>
        IEnumerable<PrintLog> GetLogs();
    }
}