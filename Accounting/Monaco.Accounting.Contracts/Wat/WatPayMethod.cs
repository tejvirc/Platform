namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    /// <summary>
    ///     Defines the payment method of host initiated transfers
    /// </summary>
    public enum WatPayMethod
    {
        /// <summary>
        ///     EGM must pay transfer to the credit meter
        /// </summary>
        Credit,

        /// <summary>
        ///     EGM must pay transfer by voucher
        /// </summary>
        Voucher
    }
}