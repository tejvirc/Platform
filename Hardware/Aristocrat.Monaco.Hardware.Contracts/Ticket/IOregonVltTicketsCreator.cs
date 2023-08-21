namespace Aristocrat.Monaco.Hardware.Contracts.Ticket
{
    /// <summary>
    ///     Definition of the IOregonVltTicketsCreator interface.
    /// </summary>
    public interface IOregonVltTicketsCreator
    {
        /// <summary>
        ///     Creates the ticket.
        /// </summary>
        /// <param name="ticketType">Type of the ticket.</param>
        /// <param name="page">Page of the ticket.</param>
        /// <returns>A ticket of the requested type</returns>
        Ticket CreateTicket(string ticketType, int page = 1);
    }
}