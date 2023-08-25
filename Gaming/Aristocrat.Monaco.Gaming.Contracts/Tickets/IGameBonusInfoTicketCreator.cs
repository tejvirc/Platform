namespace Aristocrat.Monaco.Gaming.Contracts.Tickets
{
    using System.Collections.Generic;
    using Bonus;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IGameBonusInfoTicketCreator interface.
    /// </summary>
    public interface IGameBonusInfoTicketCreator
    {
        /// <summary>
        ///     Creates a ticket containing game bonus info.
        /// </summary>
        /// <param name="bonusInfo"></param>
        /// <param name="items"></param>
        /// <returns>A Ticket object with fields required for an EGM game bonus info ticket.</returns>
        Ticket Create(string bonusInfo, IEnumerable<BonusInfoMeter> items);
    }
}
