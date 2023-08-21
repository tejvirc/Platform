namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     Published when a bonus has been successfully paid
    /// </summary>
    public class BonusAwardedEvent : BaseBonusEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusAwardedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The bonus transaction</param>
        public BonusAwardedEvent(BonusTransaction transaction)
            : base(transaction)
        {
        }
    }
}