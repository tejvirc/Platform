namespace Aristocrat.Monaco.Accounting.UI
{
    /// <summary>
    ///     Holds transaction history data as strings
    /// </summary>
    public class TransactionHistoryStrings
    {
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the sequence number.
        /// </summary>
        public string SequenceNumber { get; set; }

        /// <summary>
        ///     Gets or sets the transaction type.
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        ///     Gets the amount of cashable amount
        /// </summary>
        public string CashableAmount { get; set; }

        /// <summary>
        ///     Gets the amount of promotional credits
        /// </summary>
        public string PromotionalAmount { get; set;  }

        /// <summary>
        ///     Gets the amount of non-cashable credits
        /// </summary>
        public string NonCashableAmount { get; set;  }

        /// <summary>
        ///     Gets or sets the date and time.
        /// </summary>
        public string DateAndTime { get; set; }

        /// <summary>
        ///     Gets or sets the Validation Id.
        /// </summary>
        public string ValidationId { get; set; }

        /// <summary>
        ///     Gets or sets the Name.
        /// </summary>
        public string Name { get; set; }
    }
}