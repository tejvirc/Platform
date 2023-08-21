namespace Aristocrat.Monaco.Accounting.Contracts.Models
{
    /// <summary>
    ///     Definition of the VoucherData class.
    ///     This class stores one voucher event.
    /// </summary>
    public class VoucherData 
    {
        /// <summary>
        ///     Gets or sets the transaction Id used when this transaction is saved.
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        ///  Gets or sets sequence number
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets Voucher sequence number
        /// </summary>
        public long VoucherSequence { get; set; }

        /// <summary>
        ///     Gets or sets the date and time the cash slip was issued.
        /// </summary>
        public string TimeStamp { get; set; }

        /// <summary>
        ///     Gets the name copied from a transaction
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the name copied from a transaction
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        ///     Gets and sets the validation id
        /// </summary>
        public string ValidationId { get; set; }

        /// <summary>
        ///     Gets and sets the name of TypeOfAcount
        /// </summary>
        public string TypeOfAccount { get; set; }

        /// <summary>
        ///     Gets and sets the status of voucher-in
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     Gets or set a rejection detail message
        /// </summary>
        public string Exception { get; set; }

    }
}
