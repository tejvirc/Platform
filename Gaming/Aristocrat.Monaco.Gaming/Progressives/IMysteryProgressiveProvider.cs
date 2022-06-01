namespace Aristocrat.Monaco.Gaming.Progressives
{
    using Contracts.Progressives;

    /// <summary>
    ///     Provides an interface for mystery progressives
    /// </summary>
    public interface IMysteryProgressiveProvider
    {
        /// <summary>
        ///     Generates a valid magic number for the given progressive level and persists it to storage.
        /// </summary>
        /// <param name="progressiveLevel">The progressive level for which to generate the magic number</param>
        /// <returns>The magic number generated</returns>
        decimal GenerateMagicNumber(ProgressiveLevel progressiveLevel);

        /// <summary>
        ///     Retrieves the magic number for a progressive level
        /// </summary>
        /// <param name="assignedProgressiveKey">The assigned progressive key for the level</param>
        /// <returns>The magic number for the progressive level</returns>
        decimal GetMagicNumber(string assignedProgressiveKey);

        /// <summary>
        ///     Determines if the progressive level has met the requirements for triggering mystery jackpot
        /// </summary>
        /// <param name="assignedProgressiveKey">The assigned progressive key for the level</param>
        /// <returns>True if mystery jackpot requirements are met, false otherwise</returns>
        bool CheckMysteryJackpot(string assignedProgressiveKey);
    }
}