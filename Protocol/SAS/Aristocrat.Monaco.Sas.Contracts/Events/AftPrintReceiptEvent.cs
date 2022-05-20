namespace Aristocrat.Monaco.Sas.Contracts.Events
{
    using Aristocrat.Monaco.Hardware.Contracts.Ticket;
    using Kernel;

    /// <summary>
    ///     Definition of the AftPrintReceiptEvent class (used for Automation ONLY!)
    /// </summary>
    public class AftPrintReceiptEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AftPrintReceiptEvent" /> class.
        /// </summary>
        /// <param name="ticket">receipt</param>
        public AftPrintReceiptEvent(Ticket ticket)
        {
            Ticket = ticket;
        }

        /// <summary>
        ///     Gets a value of printed receipt
        /// </summary>
        public Ticket Ticket { get; }
    }
}
