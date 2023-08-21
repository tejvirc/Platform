namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    /// Represents the progressive hit data to be sent to Sas in long poll (0x84).
    /// PT initiates communication with exception (0x54).
    /// Note, the exception (0x51) is sent if the amount won is a jackpot.
    /// </summary>
    public class ProgressiveHitData
    {
        /// <summary>
        /// Gets or sets Amount won, in millicent.
        /// </summary>
        public ulong Amount { get; set; }

        /// <summary>
        /// Gets or sets the progressive level.
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it is a jackpot.
        /// </summary>
        public bool IsJackpot { get; set; }

        /// <summary>
        ///     Gets or sets a progressive group number.
        /// </summary>
        public int ProgressiveGroupNumber { get; set; }

        /// <summary>
        ///     Gets or sets a transactionId.
        /// </summary>
        public long TransactionId { get; set; }
    }
}
