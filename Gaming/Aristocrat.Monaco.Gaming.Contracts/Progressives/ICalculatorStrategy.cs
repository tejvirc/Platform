namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     The ICalculatorStrategy interface provides methods for processing jackpots
    /// </summary>
    public interface ICalculatorStrategy
    {
        /// <summary>
        ///     Applies the provided increment to the specified game and denom
        /// </summary>
        /// <param name="level">The level that the contribution is being applied to</param>
        /// <param name="levelUpdate">The level update that describes the the update</param>
        void ApplyContribution(ProgressiveLevel level, ProgressiveLevelUpdate levelUpdate);

        /// <summary>
        ///     Applies the provided increment to the specified game and denom
        /// </summary>
        /// <param name="level">The level that the contribution is being applied to</param>
        /// <param name="wager">The wager amount</param>
        /// <param name="ante">The ante amount</param>
        void Increment(ProgressiveLevel level, long wager, long ante);

        /// <summary>
        ///     Resets the progressive level
        /// </summary>
        /// <param name="level">The level that the contribution is being reset</param>
        void Reset(ProgressiveLevel level);

        /// <summary>
        ///     Resets the progressive level
        /// </summary>
        /// <param name="level">The level that the contribution is being reset</param>
        /// <param name="resetValue">The reset value to use</param>
        void Reset(ProgressiveLevel level, long resetValue);

        /// <summary>
        ///     Claims the progressive level
        /// </summary>
        /// <param name="level">The level that the contribution is being claimed</param>
        long Claim(ProgressiveLevel level);

        /// <summary>
        ///     Claims the progressive level
        /// </summary>
        /// <param name="level">The level that the contribution is being claimed</param>
        /// <param name="resetValue">The reset value to use</param>
        /// <returns>The won amount that was claimed</returns>
        long Claim(ProgressiveLevel level, long resetValue);
    }
}