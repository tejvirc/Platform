namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using Contracts.Models;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;

    public class GameInfoTicketCreator : IGameInfoTicketCreator, IService
    {
        private const int ItemsPerPage = 10;

        public Ticket Create()
        {
            var ticket = new GameInfoTicket();
            return ticket.CreateTextTicket();
        }

        public List<Ticket> Create(List<GameOrderData> games)
        {
            var result = new List<Ticket>();
            var pages = games.Count / ItemsPerPage + 1;
            var multiPage = pages > 1;
            var index = 0;
            for (var i = 1; i <= pages; ++i)
            {
                var count = index + ItemsPerPage > games.Count ? games.Count % ItemsPerPage : ItemsPerPage;
                var items = games.GetRange(index, count);
                index += count;
                var ticket = new GameInfoTicket(items, i, multiPage);
                result.Add(ticket.CreateTextTicket());
            }

            return result;
        }

        public string Name => "Gaming Machine Info Ticket Creator";

        public ICollection<Type> ServiceTypes => new[] { typeof(IGameInfoTicketCreator) };

        /// <summary>
        /// </summary>
        public void Initialize()
        {
        }
    }
}