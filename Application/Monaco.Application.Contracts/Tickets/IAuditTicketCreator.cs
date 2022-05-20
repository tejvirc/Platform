namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IAuditTicketCreator interface.
    /// </summary>
    public interface IAuditTicketCreator
    {
        /// <summary>
        ///     Creates an audit slip ticket.
        /// </summary>
        /// <param name="door">The door that was accessed</param>
        /// <returns>A Ticket object with fields required for an audit slip ticket.</returns>
        Ticket CreateAuditTicket(string door);
    }
}