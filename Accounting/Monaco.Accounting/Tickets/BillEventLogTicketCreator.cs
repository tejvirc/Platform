namespace Aristocrat.Monaco.Accounting.Tickets
{
    using System;
    using System.Collections.Generic;
    using Application.Tickets;
    using Contracts;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     This class creates bill event log ticket objects
    /// </summary>
    [CLSCompliant(false)]
    public class BillEventLogTicketCreator : EventLogTicketCreator, IBillEventLogTicketCreator
    {
        public Ticket Create(IList<BillTransaction> events)
        {
            return CreateTicket(events.Count, new BillEventLogTicket(events, EventsPerPage, 1));
        }

        public override int ItemLineLength => 4;

        /// <inheritdoc />
        public override string Name => "Bill Event Log Ticket Creator";

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes => new[] { typeof(IBillEventLogTicketCreator) };
    }
}