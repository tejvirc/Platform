namespace Aristocrat.Monaco.Accounting.Contracts.Tickets
{
    using System.Collections.Generic;
    using Application.Contracts.Tickets;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IBillEventLogTicketCreator interface.
    /// </summary>
    public interface IBillEventLogTicketCreator : IEventLogTicketCreator
    {
        /// <summary>
        ///     Creates a bill event log ticket.
        /// </summary>
        /// <param name="billEventLogEntries">list of BillEventMessage records to be printed</param>
        /// <returns>A Ticket object with fields required for a bill event log ticket.</returns>
        Ticket Create(IList<BillTransaction> billEventLogEntries);
    }
}