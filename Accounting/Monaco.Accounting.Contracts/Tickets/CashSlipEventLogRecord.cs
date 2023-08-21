namespace Aristocrat.Monaco.Accounting.Contracts.Tickets
{
    /// <summary>
    ///     An entry in the Cash Slip Event Log to be printed
    /// </summary>
    public struct CashSlipEventLogRecord
    {
        /// <summary>
        ///     Gets or sets the sequence number of the cash slip.
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        ///     Gets or sets the amount on the cash slip.
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        ///     Gets or sets the date and time the cash slip was issued.
        /// </summary>
        public string TimeStamp { get; set; }

        /// <summary>
        ///     Gets or sets the sequence number of the cash slip.
        /// </summary>
        public string Barcode { get; set; }
    }
}