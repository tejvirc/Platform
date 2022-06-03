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
        decimal GenerateMagicNumber(IViewableProgressiveLevel progressiveLevel);

        /// <summary>
        ///     Retrieves the magic number for a progressive level
        /// </summary>
        /// <param name="progressiveLevel">The progressive level for which to retrieve the magic number</param>
        /// <param name="magicNumber">The magic number for the level, if found</param>
        /// <returns>True if the progressive level has a magic number associated, otherwise false</returns>
        bool TryGetMagicNumber(IViewableProgressiveLevel progressiveLevel, out decimal magicNumber);

        /// <summary>
        ///     Determines if the progressive level has met the requirements for triggering mystery jackpot
        /// </summary>
        /// <param name="progressiveLevel">The progressive level to check</param>
        /// <returns>True if mystery jackpot requirements are met, false otherwise</returns>
        bool CheckMysteryJackpot(IViewableProgressiveLevel progressiveLevel);
    }
}