namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     The <see cref="ProgressiveCommitEvent" /> when the jackpot transaction is committed
    /// </summary>
    public class ProgressiveCommitEvent : ProgressiveBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveCommitEvent" /> class.
        /// </summary>
        /// <param name="jackpot">The associated jackpot transaction</param>
        /// <param name="level">The associated level</param>
        public ProgressiveCommitEvent(JackpotTransaction jackpot, IViewableProgressiveLevel level)
            : base(jackpot)
        {
            Level = level;
        }

        /// <summary>
        ///     Gets the level that was hit
        /// </summary>
        public IViewableProgressiveLevel Level { get; }
    }
}