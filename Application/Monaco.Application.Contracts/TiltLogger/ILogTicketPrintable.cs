namespace Aristocrat.Monaco.Application.Contracts.TiltLogger
{
    using System.Collections.Generic;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Interface for Log for which ticket printing (re-printing) is supported.
    /// </summary>
    public interface ILogTicketPrintable
    {
        /// <summary>
        ///     Gets Log tickets for list of transaction IDs
        /// </summary>
        /// <param name="transactionIDs"></param>
        /// <returns>A list of tickets generated from input transaction IDs</returns>
        IEnumerable<Ticket> GenerateLogTickets(IEnumerable<long> transactionIDs);

        /// <summary>
        ///     Gets ticket for a selected item
        /// </summary>
        /// <returns>A ticket corresponding to selected item.</returns>
        Ticket GetSelectedTicket(EventDescription selectedRow);

        /// <summary>
        ///     Gets whether reprint of the ticket/log is supported
        /// </summary>
        /// <returns>True if reprint supported, else false</returns>
        bool IsReprintSupported();

        /// <summary>
        ///     Gets ticket for a particular transaction ID
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns>Ticket corresponding to input transaction ID</returns>
        Ticket GenerateReprintTicket(long transactionId);
    }
}
