namespace Aristocrat.Monaco.Gaming.VideoLottery.Tickets
{
    using System;
    using System.Collections.Generic;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;

    /// <summary>
    ///     This class creates information ticket objects
    /// </summary>
    public class HelplineTicketCreator : IHelplineTicketCreator, IService
    {
        public Ticket Create(string title)
        {
            var ticket = new HelplineTicket();
            return ticket.CreateTextTicket();
        }

        /// <summary>
        ///     Gets the name of the service
        /// </summary>
        public string Name => "Message Ticket Creator";

        /// <summary>
        ///     Gets the interface this service implements and exposes as a service
        /// </summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(IHelplineTicketCreator) };

        /// <summary>
        ///     Initializes the service
        /// </summary>
        public void Initialize()
        {
        }
    }
}
