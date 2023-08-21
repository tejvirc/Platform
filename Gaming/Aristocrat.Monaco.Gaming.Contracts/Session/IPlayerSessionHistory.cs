namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to interact with the player session history
    /// </summary>
    public interface IPlayerSessionHistory
    {
        /// <summary>
        ///     Gets the current log
        /// </summary>
        IPlayerSessionLog CurrentLog { get; }

        /// <summary>
        ///     Gets the total number of entries before queue-cycling
        /// </summary>
        int MaxEntries { get; }

        /// <summary>
        ///     Gets the log sequence number of the most recent game history record within the log; 0 (zero) if no records.
        /// </summary>
        long LogSequence { get; }

        /// <summary>
        ///     Gets the total number of entries within the log.
        /// </summary>
        int TotalEntries { get; }

        /// <summary>
        ///     Returns history of player sessions
        /// </summary>
        IEnumerable<IPlayerSessionLog> GetHistory();

        /// <summary>
        ///     Gets the log for the specified transaction identifier if it exists
        /// </summary>
        /// <param name="transactionId">The transaction identifier</param>
        /// <returns>An <see cref="IPlayerSessionLog"/> if it exists</returns>
        IPlayerSessionLog GetByTransactionId(long transactionId);

        /// <summary>
        ///     Add the current player session to the history log
        /// </summary>
        void AddLog(IPlayerSessionLog playerSession);

        /// <summary>
        ///     Updates the player session log entry
        /// </summary>
        /// <param name="playerSessionLog"></param>
        void UpdateLog(IPlayerSessionLog playerSessionLog);
    }
}
