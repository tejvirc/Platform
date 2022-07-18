namespace Aristocrat.Sas.Client.Eft
{
    /// <summary>
    ///     (From section 8.EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     <para>The Eft Transfer data used in U and D type LPs to represent incoming host commands</para>
    /// </summary>
    public class EftTransferData : LongPollData
    {
        /// <summary>Long Poll command.</summary>
        public LongPoll Command { get; set; }

        /// <summary>Type of Eft Transfer</summary>
        public EftTransferType TransferType { get; set; }

        /// <summary>Acknowledgement flag.</summary>
        public bool Acknowledgement { get; set; }

        /// <summary>Transaction number from 01 -> FF.</summary>
        public int TransactionNumber { get; set; }

        /// <summary>Amount to be transferred to the gaming machine.</summary>
        public ulong TransferAmount { get; set; }

        public EftTransactionResponse ToEftTransactionResponse()
        {
            return new EftTransactionResponse
            {
                Acknowledgement = Acknowledgement,
                TransferAmount = TransferAmount,
                TransactionNumber = TransactionNumber
            };
        }
    }
}