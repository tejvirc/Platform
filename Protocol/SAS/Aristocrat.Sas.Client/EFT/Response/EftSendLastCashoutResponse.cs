namespace Aristocrat.Sas.Client.EFT.Response
{
    using Eft;

    /// <summary>
    ///     (From section 8.9 EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     <para>The Eft send last (player initiated ) cashout info to the host</para>
    /// </summary>
    public class EftSendLastCashoutResponse : LongPollResponse
    {
        /// <summary>Acknowledgement flag</summary>
        public bool Acknowledgement { get; set; }

        /// <summary>Gaming machine status (01-0E)</summary>
        public TransactionStatus Status { get; set; }

        /// <summary>Amount to be reported after player initiated cashout.</summary>
        public ulong LastCashoutAmount { get; set; }
    }
}