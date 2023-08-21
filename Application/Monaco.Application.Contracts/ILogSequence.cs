namespace Aristocrat.Monaco.Application.Contracts
{
    /// <summary>
    ///     Provides a mechanism to get or set a log sequence number, which is typically associated to a transaction
    /// </summary>
    /// <remarks>
    ///     A unique log sequence number, logSequence, is also assigned to each log record. The EGM MUST generate the log
    ///     sequence numbers as a sequence that strictly increases by 1 (one) starting at 1 (one). The EGM MUST maintain the
    ///     counters used to generate log sequence numbers in persistent storage.A separate counter MUST be used for each log.
    ///     Within a single transaction log, the log sequence numbers MUST appear as an unbroken series that strictly increases
    ///     by 1 (one). Each log record in each log MUST retain the same logSequence value the entire time that the record is
    ///     present in the log.
    /// </remarks>
    public interface ILogSequence
    {
        /// <summary>
        ///     Gets or sets the unique log sequence number assigned by the EGM; a series that strictly increases by 1 (one)
        ///     starting at 1 (one).
        /// </summary>
        long LogSequence { get; set; }
    }
}