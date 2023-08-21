namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     Published when a bonus award fails to be paid
    /// </summary>
    public class BonusFailedEvent : BaseBonusEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusFailedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The bonus transaction</param>
        public BonusFailedEvent(BonusTransaction transaction)
            : base(transaction)
        {
        }
    }
}