namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     Published when a bonus payment is requested
    /// </summary>
    public class BonusPendingEvent : BaseBonusEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusPendingEvent" /> class.
        /// </summary>
        /// <param name="transaction">The <see cref="BonusTransaction" /> associated with the event.</param>
        public BonusPendingEvent(BonusTransaction transaction)
            : base(transaction)
        {
        }
    }
}