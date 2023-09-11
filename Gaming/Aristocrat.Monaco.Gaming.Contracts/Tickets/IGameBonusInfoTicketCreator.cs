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
        /// <param name="labelResourceKeys">
        /// The resource keys for the labels of the game bonus info meter category name and total meter name to be printed
        /// <param name="items">
        /// The meters associated with the bonusInfoType
        /// </param>
        /// <returns>
        /// A Ticket object with fields required for an EGM game bonus info ticket.
        /// </returns>
        Ticket Create((string CategoryKey, string CategoryTotalKey) categoryLabelResourceKeys, IEnumerable<BonusInfoMeter> items);
    }
}
