namespace Aristocrat.Monaco.Gaming.Contracts.Tickets
{
    using System.Collections.Generic;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     game machine info ticket creator
    /// </summary>
    public interface IGameInfoTicketCreator
    {
        /// <summary>
        ///     Creates an game info ticket.
        /// </summary>
        /// <returns>A Ticket object with fields required for a game info ticket.</returns>
        Ticket Create();

        /// <summary>
        ///     Creates an game info ticket.
        /// </summary>
        /// <returns>A list of Ticket objects with fields required for an game info ticket.</returns>
        List<Ticket> Create(List<Models.GameOrderData> games);
    }
}
