namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using Kernel;

    /// <summary>
    /// Use this event to "print" a fake ticket with a message box
    /// </summary>
    public class PrintFakeTicketEvent : BaseEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PrintFakeTicketEvent(string ticketText)
        {
            TicketText = ticketText;
        }

        /// <summary>
        /// Ticket text to print
        /// </summary>
        public string TicketText { get; }
    }
}
