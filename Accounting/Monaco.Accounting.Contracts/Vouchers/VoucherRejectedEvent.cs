namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     This event is posted when a voucher redemption has been denied
    /// </summary>
    [ProtoContract]
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
        [ProtoMember(1)]
        public VoucherInTransaction Transaction { get; }
    }
}