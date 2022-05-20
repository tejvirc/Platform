namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a game configuration mapping between variation and denom
    /// </summary>
    public class GameConfigurationMap
    {
        /// <summary>
        ///     Gets or sets a value indicating whether this mapping is active. 
        ///     Inactive mappings will not be available for play
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        ///     Gets or sets the denomination
        /// </summary>
        public long Denomination { get; set; }

        /// <summary>
        ///     Gets or sets the variation
        /// </summary>
        public string Variation { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this mapping is enabled by default
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not this mapping can be edited
        /// </summary>
        public bool Editable { get; set; }

        /// <summary>
        ///     Gets a list of available bet line presets
        /// </summary>
        public IEnumerable<int> BetLinePresets { get; set; }

        /// <summary>
        ///     Gets or sets the default bet line preset
        /// </summary>
        public int DefaultBetLinePreset { get; set; }
    }
}