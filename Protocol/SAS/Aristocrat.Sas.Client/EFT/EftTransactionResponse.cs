namespace Aristocrat.Sas.Client.Eft
{
    /// <summary>
    ///     (From section 8.EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     <para>The Eft Transfer Transaction response is used in U and D type LPs to represent our response back to the host</para>
    /// </summary>
    public class EftTransactionResponse : LongPollResponse
    {
        /// <summary>Acknowledgement flag.</summary>
        public bool Acknowledgement { get; set; }

        /// <summary>Gaming machine transaction status.</summary>
        public TransactionStatus Status { get; set; }

        /// <summary>Amount to be transferred to or from the gaming machine.</summary>
        public ulong TransferAmount { get; set; }

        /// <summary>Transaction number from 01 -> FF.</summary>
        public int TransactionNumber { get; set; }
    }
}