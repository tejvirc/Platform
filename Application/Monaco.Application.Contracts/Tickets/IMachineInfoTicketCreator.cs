namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using System.Collections.Generic;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     game machine info ticket creator
    /// </summary>
    public interface IMachineInfoTicketCreator
    {
        /// <summary>
        ///     Creates event log tickets.
        /// </summary>
        /// <returns>A list of Ticket object with fields required for an event log ticket.</returns>
        List<Ticket> Create();
    }
}