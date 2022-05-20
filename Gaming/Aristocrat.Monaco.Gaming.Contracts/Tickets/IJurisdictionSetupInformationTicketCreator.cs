namespace Aristocrat.Monaco.Gaming.Contracts.Tickets
{
    using System.Collections.Generic;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IJurisdictionSetupInformationTicketCreator interface.
    /// </summary>
    public interface IJurisdictionSetupInformationTicketCreator
    {
        /// <summary>
        ///     Creates tickets containing jurisdiction setup information.  Checks TicketMode.
        /// </summary>
        /// <returns>Tickets with fields required for jurisdiction setup information tickets.</returns>
        List<Ticket> CreateJurisdictionSetupInformationTicket();
    }
}
