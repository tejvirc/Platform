namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     Published when a partial bonus payment has been issued
    /// </summary>
    public class PartialBonusPaidEvent : BaseBonusEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PartialBonusPaidEvent" /> class.
        /// </summary>
        /// <param name="transaction">The bonus transaction</param>
        /// <param name="paidCashable">The total cashable amount partially paid</param>
        /// <param name="paidNonCash">The total non cash amount partially paid</param>
        /// <param name="paidPromo">The total promotional amount partially paid</param>
        public PartialBonusPaidEvent(BonusTransaction transaction, long paidCashable, long paidNonCash, long paidPromo)
            : base(transaction)
        {
            PaidCashable = paidCashable;
            PaidNonCash = paidNonCash;
            PaidPromo = paidPromo;
        }

        /// <summary>
        ///     Gets the total cashable amount partially paid
        /// </summary>
        public long PaidCashable { get; }

        /// <summary>
        ///     Gets the total non cash amount partially paid
        /// </summary>
        public long PaidNonCash { get; }

        /// <summary>
        ///     Gets the total promotional amount partially paid
        /// </summary>
        public long PaidPromo { get; }
    }
}