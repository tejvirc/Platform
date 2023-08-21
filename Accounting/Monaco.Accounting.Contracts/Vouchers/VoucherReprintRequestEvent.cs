namespace Aristocrat.Monaco.Accounting.Contracts
{
    using Kernel;

    /// <summary>
    ///     Definition of the VoucherReprintRequestEvent class
    ///     An event emitted when requesting a reprint of a cashout voucher
    /// </summary>
    public class VoucherReprintRequestEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherReprintRequestEvent" /> class.
        /// </summary>
        public VoucherReprintRequestEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherReprintRequestEvent" /> class.
        /// </summary>
        /// <param name="transaction">the ITransaction of the voucher being reprinted</param>
        public VoucherReprintRequestEvent(ITransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets or sets transaction
        /// </summary>
        public ITransaction Transaction { get; }
    }
}