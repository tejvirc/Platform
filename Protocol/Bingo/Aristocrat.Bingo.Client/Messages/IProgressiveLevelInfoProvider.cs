namespace Aristocrat.Bingo.Client.Messages
{
    /// <summary>
    ///     The provider for mapping platform progressive level Ids to progressive server progressive level Ids.
    /// </summary>
    public interface IProgressiveLevelInfoProvider
    {
        /// <summary>
        ///     Adds a progressive level information
        /// </summary>
        /// <param name="progressiveLevelId">The progressive level id on the progressive server</param>
        /// <param name="sequenceNumber">The 1-based sequence number of the progressive level on the progressive server</param>
        /// <param name="gameTitleId">The game title id associated with the progressive level info</param>
        /// <param name="denomination">The game denomination associated with the progressive level info</param>
        void AddProgressiveLevelInfo(long progressiveLevelId, int sequenceNumber, int gameTitleId, long denomination);

        /// <summary>
        ///     Gets the progressive server progressive level id for the specified platform configured level Id
        /// </summary>
        /// <param name="levelId">The 0-based platform progressive level Id configured for the progressive level</param>
        /// <returns>The progressive server progressive level Id or -1</returns>
        long GetServerProgressiveLevelId(int levelId);

        /// <summary>
        ///     Clears the existing progressive level information
        /// </summary>
        void ClearProgressiveLevelInfo();
    }
}