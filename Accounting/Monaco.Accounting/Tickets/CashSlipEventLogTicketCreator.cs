namespace Aristocrat.Monaco.Accounting.Tickets
{
    using System;
    using System.Collections.Generic;
    using Application.Tickets;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the CashSlipEventLogTicketCreator class
    /// </summary>
    [CLSCompliant(false)]
    public class CashSlipEventLogTicketCreator : EventLogTicketCreator, ICashSlipEventLogTicketCreator
    {
        /// <inheritdoc />
        public Ticket Create(int page, IList<CashSlipEventLogRecord> events)
        {
            return CreateTicket(events.Count, new CashSlipEventLogTicket(events, EventsPerPage, page));
        }

        public override int ItemLineLength => 4;

        /// <inheritdoc />
        public override string Name => "Cash Slip Event Log Ticket Creator";

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes => new[] { typeof(ICashSlipEventLogTicketCreator) };
    }
}