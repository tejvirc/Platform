namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System;

    public interface IAftHistoryLog
    {
        /// <summary>Gets or sets the transfer code.</summary>
        AftTransferCode TransferCode { get; set; }

        /// <summary>Gets or sets the TransferStatus of the Aft transfer.</summary>
        AftTransferStatusCode TransferStatus { get; set; }

        /// <summary>Gets or sets the ReceiptStatus of the Aft transfer.</summary>
        byte ReceiptStatus { get; set; }

        /// <summary>Gets or sets the transfer type of the Aft transfer.</summary>
        AftTransferType TransferType { get; set; }

        /// <summary>Gets or sets the CashableAmount of the Aft transfer.</summary>
        ulong CashableAmount { get; set; }

        /// <summary>Gets or sets the RestrictedAmount of the Aft transfer.</summary>
        ulong RestrictedAmount { get; set; }

        /// <summary>Gets or sets the Non-restrictedAmount of the Aft transfer.</summary>
        ulong NonRestrictedAmount { get; set; }

        /// <summary>Gets or sets the TransferFlags of the Aft transfer.</summary>
        AftTransferFlags TransferFlags { get; set; }

        /// <summary>Gets or sets the asset number of the Aft transfer.</summary>
        uint AssetNumber { get; set; }

        /// <summary>Gets or sets the registration key of the Aft transfer.</summary>
        byte[] RegistrationKey { get; set; }

        /// <summary>Gets or sets the transaction id of the Aft transfer.</summary>
        string TransactionId { get; set; }

        /// <summary>Gets or sets the transaction date and time</summary>
        DateTime TransactionDateTime { get; set; }

        /// <summary>Gets or sets the expiration of the Aft transfer.</summary>
        uint Expiration { get; set; }

        /// <summary>Gets or sets the pool id of the Aft transfer.</summary>
        ushort PoolId { get; set; }

        /// <summary>Cumulative cashable amount meter for transfer type, in cents, 0-9 bytes</summary>
        long CumulativeCashableAmount { get; set; }

        /// <summary>Cumulative restricted amount meter for transfer type, in cents, 0-9 bytes</summary>
        long CumulativeRestrictedAmount { get; set; }

        /// <summary>Cumulative non-restricted amount meter for transfer type, in cents, 0-9 bytes</summary>
        long CumulativeNonRestrictedAmount { get; set; }

        /// <summary>Gets or sets the transaction index for an Aft Interrogation only poll</summary>
        byte TransactionIndex { get; set; }
    }
}