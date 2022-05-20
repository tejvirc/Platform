namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     This event is posted when a voucher redemption has been denied
    /// </summary>
    [Serializable]
    public class VoucherRejectedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherRejectedEvent" /> class.
        /// </summary>
        public VoucherRejectedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherRejectedEvent" /> class.
        /// </summary>
        /// <param name="transaction">Transaction</param>
        public VoucherRejectedEvent(VoucherInTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets the transaction
        /// </summary>
        public VoucherInTransaction Transaction { get; }
    }
}