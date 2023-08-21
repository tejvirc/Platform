namespace Aristocrat.Monaco.Gaming.Contracts.Tickets
{
    using System.Collections.Generic;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IDenomMetersTicketCreator interface.
    /// </summary>
    public interface IDenomMetersTicketCreator
    {
        /// <summary>
        ///     Creates a ticket containing denom meter values.  Checks TicketMode.
        /// </summary>
        /// <param name="denomMillicents">The denomination in millicents.</param>
        /// <param name="isLifetime">True for lifetime meters; false for period meters.</param>
        /// <returns>A Ticket object with fields required for an EGM meters ticket.</returns>
        List<Ticket> CreateDenomMetersTicket(long denomMillicents, bool isLifetime);
    }
}
