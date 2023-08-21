namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     Published when a bonus payment is started
    /// </summary>
    public class BonusStartedEvent : BaseBonusEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusStartedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The <see cref="BonusTransaction" /> associated with the event.</param>
        public BonusStartedEvent(BonusTransaction transaction)
            : base(transaction)
        {
        }
    }
}