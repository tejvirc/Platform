namespace Aristocrat.Monaco.Gaming.Contracts.Barkeeper
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides us the base barkeeper settings.
    /// </summary>
    public interface IBarkeeperSettings
    {
        /// <summary>
        ///     Gets or sets if the barkeeper is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the cash in reward level.
        /// </summary>
        RewardLevel CashInRewardLevel { get; set; }

        /// <summary>
        ///     Gets or sets the coin in rate.
        /// </summary>
        CoinInRate CoinInRate { get; set; }

        /// <summary>
        ///     Gets or sets the coin in reward levels.
        /// </summary>
        IEnumerable<RewardLevel> CoinInRewardLevels { get; set; }
    }
}
