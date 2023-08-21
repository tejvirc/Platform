namespace Aristocrat.Monaco.Accounting.Contracts.Tickets
{
    using System.Collections.Generic;
    using Application.Contracts.Tickets;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the ICashSlipEventLogTicketCreator interface.
    /// </summary>
    public interface ICashSlipEventLogTicketCreator : IEventLogTicketCreator
    {
        /// <summary>
        ///     Creates a bill event log ticket.
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="events">list of CashSlipEventLogRecords to be printed</param>
        /// <returns>A Ticket object with fields required for a bill event log ticket.</returns>
        Ticket Create(int page, IList<CashSlipEventLogRecord> events);
    }
}