namespace Aristocrat.Monaco.Accounting.Contracts.Models
{
    using Handpay;

    /// <summary>
    /// Model of Handpay Data
    /// </summary> 
    public class HandpayData
    {
        /// <summary>
        ///     Gets or sets the transaction Id used when this transaction is saved.
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the sequence number
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the validation ID
        /// </summary>
        public string ValidationId { get; set; }

        /// <summary>
        /// Gets or sets the device ID
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the date and time the jackpot handpay was issued.
        /// </summary>
        public string TimeStamp { get; set; }

        /// <summary>
        ///     Gets the name copied from a transaction
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        public string CashableAmount { get; set; }

        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        public string PromoAmount { get; set; }

        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        public string NonCashAmount { get; set; }

        /// <summary>
        ///     Gets and sets the status of handpay
        /// </summary>
        public string State { get; set; }

        /// <summary>
        ///     Gets and sets the printed state of handpay
        /// </summary>
        public bool Printed { get; set; }

        /// <summary>
        ///     Gets and sets the status of jackpot handpay
        /// </summary>
        public string HandpayType { get; set; }

        /// <summary>
        /// Gets and sets the transaction
        /// </summary>
        public HandpayTransaction Transaction { get; set; }

    }
}
