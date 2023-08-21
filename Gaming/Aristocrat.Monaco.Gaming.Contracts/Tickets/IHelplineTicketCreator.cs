namespace Aristocrat.Monaco.Gaming.Contracts.Tickets
{
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IInformationTicketCreator interface.
    /// </summary>
    public interface IHelplineTicketCreator
    {
        /// <summary>
        ///     Creates an information ticket, a ticket with a common header and free-form body.
        /// </summary>
        /// <param name="body">The pre-formatted text to print in the body of the ticket</param>
        /// <returns>A Ticket object with fields required for an information ticket.</returns>
        Ticket Create(string body);
    }
}
