namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///      Data holder class for LP 71 - Redeem Ticket
    /// </summary>
    public class RedeemTicketData : LongPollData
    {
        /// <summary>
        ///     The value used to represent no pool is provided
        /// </summary>
        public const int NilPoolId = 0;

        /// <summary>
        ///     Gets or sets the ticket transfer code 
        /// </summary>
        public TicketTransferCode TransferCode { get; set; }

        /// <summary>
        ///     Gets or sets the ticket transfer amount
        /// </summary>
        public ulong TransferAmount { get; set; }

        /// <summary>
        ///     Gets or sets the parsing code
        /// </summary>
        public ParsingCode ParsingCode { get; set; }

        /// <summary>
        ///     Gets or sets the barcode
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        ///     Gets or sets the restricted credit expiration date
        /// </summary>
        public long RestrictedExpiration { get; set; }

        /// <summary>
        ///     Gets or sets the restricted credit pool ID
        /// </summary>
        public int PoolId { get; set; }

        /// <summary>
        ///     Gets or sets the target ID
        /// </summary>
        public string TargetId { get; set; }
    }

    /// <summary>
    ///     Response holder class for LP 71 - Redeem Ticket
    /// </summary>
    public class RedeemTicketResponse : LongPollResponse
    {
        /// <summary>
        ///     Gets or sets the redemption status code
        /// </summary>
        public RedemptionStatusCode MachineStatus { get; set; }

        /// <summary>
        ///     Gets or sets the ticket transfer amount
        /// </summary>
        public ulong TransferAmount { get; set; }

        /// <summary>
        ///     Gets or sets the parsing code
        /// </summary>
        public ParsingCode ParsingCode { get; set; }

        /// <summary>
        ///     Gets or sets the barcode
        /// </summary>
        public string Barcode { get; set; }
    }
}
