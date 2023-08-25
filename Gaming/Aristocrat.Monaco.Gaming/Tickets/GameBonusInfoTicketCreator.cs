namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using Contracts.Bonus;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;

    /// <summary>
    ///     This class creates information ticket objects
    /// </summary>
    public class GameBonusInfoTicketCreator : IGameBonusInfoTicketCreator, IService
    {
        public Ticket Create(
            string bonusInfo,
            IEnumerable<BonusInfoMeter> items)
        {
            var ticket = new GameBonusInfoTicket(bonusInfo, items);
            
            return ticket.CreateTextTicket();
        }

        /// <summary>
        ///     Gets the name of the service
        /// </summary>
        public string Name => "Game Bonus Info Ticket Creator";

        /// <summary>
        ///     Gets the interface this service implements and exposes as a service
        /// </summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameBonusInfoTicketCreator) };

        /// <summary>
        ///     Initializes the service
        /// </summary>
        public void Initialize()
        {
        }
    }
}
