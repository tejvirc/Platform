namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     The <see cref="DisplayLimitExceededEvent" /> is posted when the bonus award amount exceeds the specified display
    ///     limit
    /// </summary>
    public class DisplayLimitExceededEvent : BaseBonusEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayLimitExceededEvent" /> class.
        /// </summary>
        /// <param name="transaction">The bonus transaction</param>
        public DisplayLimitExceededEvent(BonusTransaction transaction)
            : base(transaction)
        {
        }
    }
}