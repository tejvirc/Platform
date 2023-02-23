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
        /// <param name="sequenceNumber">The 1-based sequence number of the progressive level on the progressive server</param>
        void AddProgressiveLevelInfo(long progressiveLevelId, int sequenceNumber);

        /// <summary>
        ///     Gets the progressive server progressive level id for the specified platform configured progressive Id
        /// </summary>
        /// <param name="progressiveId">The 0-based progressive Id configured for the platform progressive level</param>
        /// <returns>The progressive server progressive level Id or -1</returns>
        long GetProgressiveLevelId(int progressiveId);

        /// <summary>
        ///     Gets the progressive server progressive sequence number for the specified progressive server progressive level id
        /// </summary>
        /// <param name="progressiveLevelId">The progressive level id from the progressive server</param>
        /// <returns>The progressive server progressive sequence number or -1</returns>
        int GetProgressiveSequenceNumber(long progressiveLevelId);

        /// <summary>
        ///     Clears the existing progressive level information
        /// </summary>
        void ClearProgressiveLevelInfo();
    }
}