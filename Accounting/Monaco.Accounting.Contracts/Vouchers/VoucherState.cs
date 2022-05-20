namespace Aristocrat.Monaco.Accounting.Contracts
{
    /// <summary>
    ///     Defines the current state of a voucher
    /// </summary>
    public enum VoucherState
    {
        /// <summary>
        ///     Voucher issued
        /// </summary>
        Issued,

        /// <summary>
        ///     Redemption requested
        /// </summary>
        Pending,

        /// <summary>
        ///     Voucher stacked
        /// </summary>
        Redeemed,

        /// <summary>
        ///     Voucher was rejected
        /// </summary>
        Rejected
    }
}