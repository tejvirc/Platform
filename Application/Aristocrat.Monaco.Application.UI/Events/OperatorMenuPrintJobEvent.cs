namespace Aristocrat.Monaco.Application.UI.Events
{
    using Hardware.Contracts.Ticket;
    using Kernel;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the OperatorMenuPrintJobEvent class.
    ///     <remarks>
    ///         This event is posted by Operator Menu pages when they
    ///         want to print tickets.  The OperatorMenuPrintHandler 
    ///         listens to these events and will print out the tickets 
    ///         passed in the event.
    ///     </remarks>
    /// </summary>
    [Serializable]
    public class OperatorMenuPrintJobEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuPrintJobEvent" /> class.
        /// </summary>
        public OperatorMenuPrintJobEvent(IEnumerable<Ticket> ticketsToPrint)
        {
            TicketsToPrint = ticketsToPrint ?? new List<Ticket>();
        }

        /// <summary>
        ///     Gets or sets field of interest
        /// </summary>
        public IEnumerable<Ticket> TicketsToPrint { get;}
    }
}