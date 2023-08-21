namespace Aristocrat.Monaco.Hardware.Contracts.TicketContent
{
    using Ticket;

    /// <summary>Definition of the ITemplateRenderer interface.</summary>
    public interface ITemplateRenderer
    {
        /// <summary>Transform the ticket data into a TCL print command</summary>
        /// <param name="ticket">The ticket to render</param>
        /// <param name="resolver">The resolver that has loaded the templates and regions</param>
        /// <param name="printCommand">
        ///     This string will be populated with the TCL command to print the data in the ticket
        /// </param>
        /// <param name="adjustTextTicketTitle">Indicates whether or not to adjust the text ticket title</param>
        /// <returns>true if successful, false if unrecognized ticket type</returns>
        bool RenderTicket(Ticket ticket, IResolver resolver, out string printCommand, bool adjustTextTicketTitle = false);
    }
}
