namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System;

    /// <summary>
    ///     Holds the data used to print Aft Receipts
    /// </summary>
    public class AftReceiptData
    {
        /// <summary>Gets or sets the transfer source.</summary>
        public string TransferSource { get; set; } = string.Empty;

        /// <summary> Gets or sets the receipt time. </summary>
        public DateTime ReceiptTime { get; set; }

        /// <summary>Gets or sets the patron name.</summary>
        public string PatronName { get; set; } = string.Empty;

        /// <summary>Gets or sets the patron account.</summary>
        public string PatronAccount { get; set; } = string.Empty;

        /// <summary>Gets or sets the last 4 digits of the debit card number.</summary>
        public string DebitCardNumber { get; set; } = string.Empty;

        /// <summary>Gets or sets the account balance before the transaction</summary>
        public ulong AccountBalance { get; set; }

        /// <summary>Gets or sets the transaction fee in cents</summary>
        public ulong TransactionFee { get; set; }

        /// <summary>Gets or sets the total debit amount in cents</summary>
        public ulong DebitAmount { get; set; }
    }
}