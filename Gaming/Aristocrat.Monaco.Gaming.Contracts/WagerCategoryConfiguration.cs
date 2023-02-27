namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Wager category configuration
    /// </summary>
    public class WagerCategoryConfiguration
    {
        /// <summary>
        ///     Gets or sets the identifier for the wager category
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the theoretical payback rtp
        /// </summary>
        public decimal TheoPaybackPercentRtp { get; set; }
    }
}