namespace Aristocrat.Monaco.Sas
{
    using Contracts.Client;

    /// <inheritdoc />
    public class NoTicketPrintedHandler : ISasTicketPrintedHandler
    {
        /// <inheritdoc />
        public void TicketPrintedAcknowledged()
        {
            // Do nothing as the client with this handler will not interact with ticket
        }

        /// <inheritdoc />
        public void ClearPendingTicketPrinted()
        {
            // Do nothing as the client with this handler will not interact with ticket
        }

        /// <inheritdoc />
        public void ProcessPendingTickets()
        {
            // Do nothing as the client with this handler will not interact with ticket
        }

        /// <inheritdoc />
        public long PendingTransactionId { get; set; }
    }
}