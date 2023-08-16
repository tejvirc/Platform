namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event emitted when a Voucher Out has completed.
    /// </summary>
    [ProtoContract]
    public class VoucherIssuedEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public VoucherIssuedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherIssuedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        /// <param name="ticket">The associated ticket</param>
        public VoucherIssuedEvent(VoucherOutTransaction transaction, Ticket ticket)
        {
            Transaction = transaction;
            PrintedTicket = ticket;
        }

        /// <summary>
        ///     Gets the associated transaction
        /// </summary>
        [ProtoMember(1)]
        public VoucherOutTransaction Transaction { get; }

        ///<summary>
        ///     Gets the associated ticket
        /// </summary>
        [ProtoMember(2)]
        public Ticket PrintedTicket { get; }
    }
}