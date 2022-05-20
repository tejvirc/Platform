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
        public ProgressiveHitEvent(JackpotTransaction jackpot, IViewableProgressiveLevel level, bool isRecovery)
            : base(jackpot)
        {
            Level = level;
            IsRecovery = isRecovery;
        }

        /// <summary>
        ///     Gets the level that was hit
        /// </summary>
        public IViewableProgressiveLevel Level { get; }

        /// <summary>
        ///     Gets whether or not we recovering
        /// </summary>
        public bool IsRecovery { get; }
    }
}