namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a game configuration restriction
    /// </summary>
    public class GameConfiguration
    {
        /// <summary>
        ///     Gets or sets the game Theme Name
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        ///     Gets or sets the game Long Theme Name
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the maximum payback pct.
        /// </summary>
        /// <value>
        ///     Maximum theoretical payback percentage for the game; a value of 0 (zero) indicates that the information is not
        ///     available; otherwise, it MUST be set to the maximum payback percentage of the game, which MUST be greater than 0
        ///     (zero). For example, a value of 96371 represents a maximum payback percentage of 96.371%.
        /// </value>
        public long MaxPaybackPercent { get; set; }

        /// <summary>
        ///     Gets or sets the minimum payback pct.
        /// </summary>
        /// <value>
        ///     Minimum theoretical payback percentage for the game; a value of 0 (zero) indicates that the information is not
        ///     available; otherwise, it MUST be set to the minimum payback percentage for the game, which MUST be greater than 0
        ///     (zero) and less than or equal to maxPaybackPct. For example, a value of 82451 represents a minimum payback
        ///     percentage of 82.451%.
        /// </value>
        public long MinPaybackPercent { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this configuration can be overridden by the operator or host
        /// </summary>
        /// <value><c>true</c> if editable; otherwise, <c>false</c>.</value>
        public bool Editable { get; set; }

        /// <summary>
        ///     Gets or sets a collection configuration mappings
        /// </summary>
        /// <value>The configuration mapping.</value>
        public IEnumerable<GameConfigurationMap> ConfigurationMapping { get; set; }

        /// <summary>
        ///     Gets or sets the maximum denoms enabled.
        /// </summary>
        /// <value>The maximum denoms that can be enabled by the Operator.</value>
        public int? MaxDenomsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the minimum denom count.
        /// </summary>
        /// <value>The minimum denoms enabled.</value>
        public int MinDenomsEnabled { get; set; }
    }
}