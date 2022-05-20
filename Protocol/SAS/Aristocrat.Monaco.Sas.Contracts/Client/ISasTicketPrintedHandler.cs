namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    ///     The ticket printed handler for SAS
    /// </summary>
    public interface ISasTicketPrintedHandler
    {
        /// <summary>
        ///     SAS has acknowledged the ticket validation information
        /// </summary>
        void TicketPrintedAcknowledged();

        /// <summary>
        ///     Clears any pending ticket printed waiting acknowledgement
        /// </summary>
        void ClearPendingTicketPrinted();

        /// <summary>
        ///     Notifies the SAS ticket printed handler to process any pending tickets
        /// </summary>
        void ProcessPendingTickets();

        /// <summary>
        ///     Gets or sets the pending transactionId to be marked read
        /// </summary>
        long PendingTransactionId { get; set; }
    }
}