namespace Aristocrat.Monaco.Gaming.Contracts.Barkeeper
{
    using System.Collections.Generic;

    /// <summary>
    ///     Stores data for barkeeper settings.
    /// </summary>
    public class BarkeeperStorageData : IBarkeeperSettings
    {
        /// <summary>
        ///     Gets or sets the coin in.
        /// </summary>
        public long CoinIn { get; set; }

        /// <summary>
        ///     Gets or sets the cash in.
        /// </summary>
        public long CashIn { get; set; }

        /// <summary>
        ///     Get or Sets BarkeeperRewardLevels
        /// </summary>
        public BarkeeperRewardLevels BarkeeperRewardLevels { get; set; }

        /// <summary>
        ///     Active Coin In level
        /// </summary>
        public RewardLevel ActiveCoinInLevel { get; set; }

        /// <summary>
        ///     Gets or Sets the current Rate Of Play elapsed milliseconds
        /// </summary>
        public long RateOfPlayElapsedMilliseconds { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public RewardLevel CashInRewardLevel { get; set; }

        /// <inheritdoc />
        public CoinInRate CoinInRate { get; set; }

        /// <inheritdoc />
        public IEnumerable<RewardLevel> CoinInRewardLevels { get; set; }
    }
}