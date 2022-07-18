namespace Aristocrat.Monaco.Sas.Eft
{
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.Eft;
    using System;

    /// <inheritdoc />
    public class EftHistoryLogEntry : IEftHistoryLogEntry
    {
        /// <inheritdoc />
        public LongPoll Command { get; set; }

        /// <inheritdoc />
        public int TransactionNumber { get; set; }

        /// <inheritdoc />
        public DateTime TransactionDateTime { get; set; }

        /// <inheritdoc />
        public ulong RequestedTransactionAmount { get; set; }

        /// <inheritdoc />
        public ulong ReportedTransactionAmount { get; set; }

        /// <inheritdoc />
        public bool Acknowledgement { get; set; }

        /// <inheritdoc />
        public TransactionStatus ReportedTransactionStatus { get; set; }

        /// <inheritdoc />
        public bool ToBeProcessed { get; set; }

        /// <inheritdoc />
        public EftTransferType TransferType { get; set; }
    }
}
