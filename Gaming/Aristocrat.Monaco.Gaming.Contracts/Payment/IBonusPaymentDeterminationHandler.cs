namespace Aristocrat.Monaco.Gaming.Contracts.Payment
{
    using Bonus;

    /// <summary>
    ///     An interface that we can use in order to decide what to do with a bonus payout depending on rules that may be
    ///     protocol or jurisdiction specific.
    /// </summary>
    public interface IBonusPaymentDeterminationHandler
    {
        /// <summary>
        ///     Return the results of paying a bonus amount. This is used for to figure out where the bonus award should go - to
        ///     the credit meter, to a handpay, to a voucher, etc.
        /// </summary>
        /// <param name="transaction">The bonus transaction associated with this award.</param>
        /// <param name="bonusAmountInMillicents">The bonus amount that has to be paid.</param>
        /// <returns>Pay method denoting the destination for this amount.</returns>
        PayMethod GetBonusPayMethod(BonusTransaction transaction, long bonusAmountInMillicents);
    }
}