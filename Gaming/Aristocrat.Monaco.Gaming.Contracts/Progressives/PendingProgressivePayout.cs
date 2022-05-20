namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     A pending progressive payout is used when committing a progressive win
    ///     and defines the data required to complete the payout.
    /// </summary>
    public class PendingProgressivePayout
    {
        /// <summary>
        ///     Gets or sets the device id for the pending progressive payout
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the level id for the pending progressive payout
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets or sets the transaction id for the pending progressive payout
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the payment method for the progressive payout
        /// </summary>
        public PayMethod PayMethod { get; set; }

        /// <summary>
        ///     Gets or sets the amount paid for the progressive payout
        /// </summary>
        public long PaidAmount { get; set; }
    }
}