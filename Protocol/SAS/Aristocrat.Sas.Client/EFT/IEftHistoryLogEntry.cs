namespace Aristocrat.Sas.Client.Eft
{
    using System;

    /// <summary>History logs of EFT transactions</summary>
    public interface IEftHistoryLogEntry
    {
        LongPoll Command { get; set; }

        /// <summary>Gets or sets the transaction id of the EFT transfer.</summary>
        int TransactionNumber { get; set; }

        /// <summary>Gets or sets the transaction date and time</summary>
        DateTime TransactionDateTime { get; set; }

        /// <summary>Gets or sets transaction amount requested from host</summary>
        ulong RequestedTransactionAmount { get; set; }

        /// <summary>Gets or sets transaction amount returned in response to host</summary>
        ulong ReportedTransactionAmount { get; set; }

        /// <summary>Ack flag</summary>
        bool Acknowledgement { get; set; }

        /// <summary>Transaction status reported in response</summary>
        TransactionStatus ReportedTransactionStatus { get; set; }

        /// <summary>Is the command ready to be processed and not processed yet</summary>
        bool ToBeProcessed { get; set; }

        /// <summary>Get or Sets the Transfer Type</summary>
        EftTransferType TransferType { get; set; }
    }
}