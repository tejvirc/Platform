namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using Hardware.Contracts.Ticket;
    using Kernel;

    /// <summary>
    ///     Definition of the IInformationTicketCreator interface.
    /// </summary>
    public interface IInformationTicketCreator : IService
    {
        /// <summary>
        ///     Creates an information ticket, a ticket with a common header and free-form body.
        /// </summary>
        /// <param name="title">The text to print in the title area of the ticket</param>
        /// <param name="body">The pre-formatted text to print in the body of the ticket</param>
        /// <returns>A Ticket object with fields required for an information ticket.</returns>
        Ticket CreateInformationTicket(string title, string body);
    }
}