namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Hardware.Contracts.Ticket;
    using Kernel;

    /// <summary>
    ///     Event emitted when a Voucher Out has completed.
    /// </summary>
    [Serializable]
    public class VoucherIssuedEvent : BaseEvent
    {
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
        public VoucherOutTransaction Transaction { get; }

        ///<summary>
        ///     Gets the associated ticket
        /// </summary>|
        public Ticket PrintedTicket { get; }
    }
}