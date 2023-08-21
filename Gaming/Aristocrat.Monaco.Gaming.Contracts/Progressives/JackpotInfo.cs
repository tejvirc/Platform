namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;

    /// <summary>
    ///     Represents jackpot data typically associated with a game round
    /// </summary>
    [Serializable]
    public class JackpotInfo
    {
        /// <summary>
        ///     Gets or sets the transaction identifier
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the JackpotHit date time.
        /// </summary>
        public DateTime HitDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the pay method.
        /// </summary>
        public PayMethod PayMethod { get; set; }

        /// <summary>
        ///     Gets or sets the device identifier
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive pack name
        /// </summary>
        public string PackName { get; set; }

        /// <summary>
        ///     Gets or sets the progressive level identifier
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets or sets the win amount
        /// </summary>
        public long WinAmount { get; set; }

        /// <summary>
        ///     Gets or sets Gets or sets the win text.
        /// </summary>
        public string WinText { get; set; }

        /// <summary>
        ///     /// <summary>
        ///     Gets the WagerCredits in cents associated with the progressive level that's hit
        /// </summary>
        public long WagerCredits { get; set; }
    }
}