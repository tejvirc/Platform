namespace Aristocrat.Monaco.Accounting.Tickets
{
    using System;
    using System.Collections.Generic;
    using Application.Tickets;
    using Contracts.Models;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     This class creates information ticket objects
    /// </summary>
    [CLSCompliant(false)]
    public class HandpayTicketCreator : EventLogTicketCreator, IHandpayTicketCreator
    {
        public Ticket Create(int page, IList<HandpayData> items)
        {
            return CreateTicket(items.Count, new HandpayTicket(items, EventsPerPage, page));
        }

        public override int ItemLineLength => 8;

        /// <summary>
        ///     Gets the name of the service
        /// </summary>
        public override string Name => "Handpay Ticket Creator";

        /// <summary>
        ///     Gets the interface this service implements and exposes as a service
        /// </summary>
        public override ICollection<Type> ServiceTypes => new[] { typeof(IHandpayTicketCreator) };
    }
}
