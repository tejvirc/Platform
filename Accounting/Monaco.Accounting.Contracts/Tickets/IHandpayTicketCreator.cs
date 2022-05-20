
namespace Aristocrat.Monaco.Accounting.Contracts.Tickets
{
    using System.Collections.Generic;
    using Application.Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Models;

    /// <summary>
    ///     Definition of the IHandpayTicketCreator interface.
    /// </summary>
    public interface IHandpayTicketCreator : IEventLogTicketCreator
    {
        /// <summary>
        ///     Creates a ticket containing game history.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="page"></param>
        /// <returns>A Ticket object with fields required for an EGM game history ticket.</returns>
        Ticket Create(int page, IList<HandpayData> items);
    }
}
