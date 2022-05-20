namespace Aristocrat.Monaco.Accounting.Contracts
{
    using Kernel;

    /// <summary>
    ///     This event is generated after the voucher validator has responded to a request to authorize the voucher
    /// </summary>
    public class VoucherAuthorizedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherAuthorizedEvent" /> class.
        /// </summary>
        /// <param name="transaction">Transaction</param>
        public VoucherAuthorizedEvent(VoucherInTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets the transaction
        /// </summary>
        public VoucherInTransaction Transaction { get; }
    }
}