namespace Aristocrat.Bingo.Client.Messages
{
    /// <summary>
    ///     The provider for the progressive level information returned from the progressive server.
    /// </summary>
    public interface IProgressiveLevelInfoProvider
    {
        /// <summary>
        ///     Adds a progressive level information
        /// </summary>
        /// <param name="progressiveLevelId">The progressive level id on the progressive server</param>
        /// <param name="sequenceNumber">The sequence number of the progressive level on the progressive server</param>
        void AddProgressiveLevelInfo(long progressiveLevelId, int sequenceNumber);

        /// <summary>
        ///     Gets the progressive level id for the specified sequence number
        /// </summary>
        /// <param name="sequenceNumber">The sequence number of the progressive level</param>
        /// <returns>The progressive level Id or -1</returns>
        long GetProgressiveLevelId(int sequenceNumber);

        /// <summary>
        ///     Gets the progressive sequence number for the specified progressive level id
        /// </summary>
        /// <param name="progressiveLevelId">The progressive level id</param>
        /// <returns>The progressive sequence number or -1</returns>
        int GetProgressiveSequenceNumber(long progressiveLevelId);

        /// <summary>
        ///     Clears the existing progressive level information
        /// </summary>
        void ClearProgressiveLevelInfo();
    }
}