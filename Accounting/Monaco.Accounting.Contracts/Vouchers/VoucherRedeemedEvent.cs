namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event emitted when a voucher redemption request has been authorized, the voucher has been redeemed, all associated
    ///     meters have been updated, and the voucher has been stacked
    /// </summary>
    [ProtoContract]
    public class VoucherRedeemedEvent : BaseEvent
    {
        /// <summary>
        /// empty constructor for deserialization
        /// </summary>
        public VoucherRedeemedEvent()
        {
        }

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
        [ProtoMember(1)]
        public VoucherInTransaction Transaction { get; }
    }
}