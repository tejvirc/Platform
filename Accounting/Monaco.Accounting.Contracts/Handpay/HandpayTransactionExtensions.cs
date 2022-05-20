namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using Contracts;
    using Kernel;

    /// <summary>
    ///     Extension methods for handpay transactions
    /// </summary>
    public static class HandpayTransactionExtensions
    {
        /// <summary>
        ///     Checks whether the handpay transaction may be reset to the credit meter
        /// </summary>
        /// <param name="this">The transaction to check</param>
        /// <param name="propertiesManager">An instance of IPropertiesManager</param>
        /// <param name="bank">An instance of IBank</param>
        /// <returns>True when the handpay transaction may be reset to the credit meter</returns>
        public static bool EligibleResetToCreditMeter(this HandpayTransaction @this, IPropertiesManager propertiesManager, IBank bank)
        {
            var handpayLimit = propertiesManager.GetValue(AccountingConstants.HandpayLimit, AccountingConstants.DefaultHandpayLimit);
            var winAmount = @this.WinAmount();
            return @this.HandpayType != HandpayType.CancelCredit &&
                   bank.QueryBalance() + winAmount <= bank.Limit &&
                   winAmount > 0 && winAmount < handpayLimit;
        }

        /// <summary>
        ///     Returns Win amount
        /// </summary>
        /// <param name="this">The transaction to check</param>
        /// <return>Win amount </return>
        public static long WinAmount(this HandpayTransaction @this)
        {
            return @this.CashableAmount + @this.PromoAmount;
        }
    }
}