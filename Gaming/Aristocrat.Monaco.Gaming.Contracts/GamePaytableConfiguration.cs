namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Server paytable configuration installation
    /// </summary>
    public class ServerPaytableConfiguration
    {
        /// <summary>
        ///     Gets or sets the paytable id for this server paytable configuration
        /// </summary>
        public string PaytableId { get; set; }

        /// <summary>
        ///     Gets or sets the minimum payback percentage
        /// </summary>
        public decimal MinimumPaybackPercent { get; set; }

        /// <summary>
        ///     Gets or sets the maximum payback percentage
        /// </summary>
        public decimal MaximumPaybackPercent { get; set; }

        /// <summary>
        ///     Gets or sets the denomination configurations
        /// </summary>
        public IEnumerable<DenominationConfiguration> DenominationConfigurations { get; set; } =
            Enumerable.Empty<DenominationConfiguration>();

        /// <summary>
        ///     Gets or sets the wager category configurations
        /// </summary>
        public IEnumerable<WagerCategoryConfiguration> WagerCategoryOptions { get; set; } =
            Enumerable.Empty<WagerCategoryConfiguration>();
    }
}