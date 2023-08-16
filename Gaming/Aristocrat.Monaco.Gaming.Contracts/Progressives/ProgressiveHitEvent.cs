namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     A <see cref="ProgressiveHitEvent"/> is posted when a progressive hit has occured
    /// </summary>
    public class ProgressiveHitEvent : ProgressiveBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveHitEvent" /> class.
        /// </summary>
        /// <param name="jackpot">The associated jackpot transaction</param>
        /// <param name="level">The progressive level that was hit</param>
        /// <param name="isRecovery">Whether or not we are processing recovery</param>
        /// <param name="remainingAmount">The remaining amount that can be claimed</param>
        public ProgressiveHitEvent(JackpotTransaction jackpot, IViewableProgressiveLevel level, bool isRecovery, long? remainingAmount = default)
            : base(jackpot)
        {
            Level = level;
            IsRecovery = isRecovery;
            RemainingAmount = remainingAmount;  
        }

        /// <summary>
        ///     Gets the level that was hit
        /// </summary>
        public IViewableProgressiveLevel Level { get; }

        /// <summary>
        ///     Gets whether or not we recovering
        /// </summary>
        public bool IsRecovery { get; }

        /// <summary>
        ///     Gets the remaining amount that can be claimed
        /// </summary>
        public long? RemainingAmount { get; }
    }
}