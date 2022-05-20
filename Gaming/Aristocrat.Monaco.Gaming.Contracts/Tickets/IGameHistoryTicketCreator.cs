namespace Aristocrat.Monaco.Gaming.Contracts.Tickets
{
    using System.Collections.Generic;
    using Hardware.Contracts.Ticket;
    using Models;

    /// <summary>
    ///     Definition of the IGameHistoryTicketCreator interface.
    /// </summary>
    public interface IGameHistoryTicketCreator
    {
        /// <summary>
        ///     Creates a ticket containing game history.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="page"></param>
        /// <returns>A Ticket object with fields required for an EGM game history ticket.</returns>
        Ticket Create(int page, IList<GameRoundHistoryItem> items);

        /// <summary>
        ///     Creates a multi page game history ticket containing game history.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>A Ticket objects with fields required for an EGM game history ticket.</returns>
        List<Ticket> CreateMultiPage(GameRoundHistoryItem item);

        /// <summary>
        ///     Checks for multi page ticket.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool MultiPage(GameRoundHistoryItem item);
    }
}
