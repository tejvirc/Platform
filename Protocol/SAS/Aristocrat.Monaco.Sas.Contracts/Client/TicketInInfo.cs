namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Definition of the TicketInInfo class.
    /// </summary>
    public class TicketInInfo : ICloneable
    {
        /// <summary>
        ///     Gets or sets the amount
        /// </summary>
        public ulong Amount { get; set; }

        /// <summary>
        ///     Gets or sets the transaction id
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the barcode
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        ///     Gets or sets the redemption status code
        /// </summary>
        public RedemptionStatusCode RedemptionStatusCode { get; set; }

        /// <summary>
        ///     Gets or sets the ticket transfer code
        /// </summary>
        public TicketTransferCode TransferCode { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return new TicketInInfo
            {
                Amount = Amount,
                TransactionId = TransactionId,
                Barcode = Barcode,
                RedemptionStatusCode = RedemptionStatusCode,
                TransferCode = TransferCode
            };
        }
    }
}
