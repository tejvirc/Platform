namespace Aristocrat.Monaco.Gaming.Progressives
{
    using Contracts.Progressives;

    /// <summary>
    ///     Provides an interface for standalone progressives
    /// </summary>
    public interface ISapProvider
    {
        /// <summary>
        ///     Increments a progressive level using a wager and ante value
        /// </summary>
        /// <param name="level">The level to increment</param>
        /// <param name="wager">The wager to be applied to the level</param>
        /// <param name="ante">The ante to be applied to the level</param>
        void Increment(ProgressiveLevel level, long wager, long ante);

        /// <summary>
        ///     Process a hit on a standalone progressive level
        /// </summary>
        /// <param name="level">The associated progressive level</param>
        /// <param name="transaction">The associated transaction</param>
        void ProcessHit(ProgressiveLevel level, IViewableJackpotTransaction transaction);

        /// <summary>
        ///     Reset the progressive level after it has been hit and awarded
        /// </summary>
        /// <param name="level">The progressive level to reset</param>
        void Reset(ProgressiveLevel level);
    }
}