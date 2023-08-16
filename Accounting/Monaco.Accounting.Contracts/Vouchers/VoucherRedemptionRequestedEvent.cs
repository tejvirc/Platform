namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event emitted when a ticket has been inserted into the note acceptor and the request
    ///     has been identified eligible for processing.
    /// </summary>
    /// <remarks>
    ///     This event only signals the start of handling a ticket-in request. It is posted before
    ///     the note acceptor starts stacking the ticket.
    /// </remarks>
    [ProtoContract]
    public class VoucherRedemptionRequestedEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public VoucherRedemptionRequestedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherRedemptionRequestedEvent" /> class.
        /// </summary>
        /// <param name="transaction">Transaction</param>
        public VoucherRedemptionRequestedEvent(VoucherInTransaction transaction)
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