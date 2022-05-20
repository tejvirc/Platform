namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Event emitted when a voucher redemption request has been authorized, the voucher has been redeemed, all associated
    ///     meters have been updated, and the voucher has been stacked
    /// </summary>
    [Serializable]
    public class VoucherRedeemedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherRedeemedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        public VoucherRedeemedEvent(VoucherInTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets the associated transaction
        /// </summary>
        public VoucherInTransaction Transaction { get; }
    }
}