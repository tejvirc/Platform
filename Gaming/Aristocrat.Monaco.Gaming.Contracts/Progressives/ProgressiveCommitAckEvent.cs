namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     The <see cref="ProgressiveCommitAckEvent"/> is posted when the host acknowledges the jackpot transaction
    /// </summary>
    public class ProgressiveCommitAckEvent : ProgressiveBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveCommitAckEvent" /> class.
        /// </summary>
        /// <param name="jackpot">The associated jackpot transaction</param>
        public ProgressiveCommitAckEvent(JackpotTransaction jackpot)
            : base(jackpot)
        {
        }
    }
}