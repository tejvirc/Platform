namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     Published when a bonus award is cancelled
    /// </summary>
    public class BonusCancelledEvent : BaseBonusEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusCancelledEvent" /> class.
        /// </summary>
        /// <param name="transaction">The bonus transaction</param>
        public BonusCancelledEvent(BonusTransaction transaction)
            : base(transaction)
        {
        }
    }
}