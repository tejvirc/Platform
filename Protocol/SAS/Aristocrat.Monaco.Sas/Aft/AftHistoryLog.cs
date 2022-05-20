namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <inheritdoc />
    public class AftHistoryLog : IAftHistoryLog
    {
        /// <inheritdoc />
        public AftTransferCode TransferCode { get; set; }

        /// <inheritdoc />
        public AftTransferStatusCode TransferStatus { get; set; }

        /// <inheritdoc />
        public byte ReceiptStatus { get; set; }

        /// <inheritdoc />
        public AftTransferType TransferType { get; set; }

        /// <inheritdoc />
        public ulong CashableAmount { get; set; }

        /// <inheritdoc />
        public ulong RestrictedAmount { get; set; }

        /// <inheritdoc />
        public ulong NonRestrictedAmount { get; set; }

        /// <inheritdoc />
        public AftTransferFlags TransferFlags { get; set; }

        /// <inheritdoc />
        public uint AssetNumber { get; set; }

        /// <inheritdoc />
        public byte[] RegistrationKey { get; set; }

        /// <inheritdoc />
        public string TransactionId { get; set; }

        /// <inheritdoc />
        public DateTime TransactionDateTime { get; set; }

        /// <inheritdoc />
        public uint Expiration { get; set; }

        /// <inheritdoc />
        public ushort PoolId { get; set; }

        /// <inheritdoc />
        public long CumulativeCashableAmount { get; set; }

        /// <inheritdoc />
        public long CumulativeRestrictedAmount { get; set; }

        /// <inheritdoc />
        public long CumulativeNonRestrictedAmount { get; set; }

        /// <inheritdoc />
        public byte TransactionIndex { get; set; }
    }
}