namespace Aristocrat.Monaco.Gaming.Contracts.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to map a denomination to a paytable
    /// </summary>
    public interface IDenomToPaytable
    {
        /// <summary>
        ///     Gets a value indicating whether or not this mapping is active and can be enabled/played
        /// </summary>
        bool Active { get; }

        /// <summary>
        ///     Gets the denomination
        /// </summary>
        long Denomination { get; }

        /// <summary>
        ///     Gets the variation Id
        /// </summary>
        string VariationId { get; }

        /// <summary>
        ///     Gets a value indicating whether or not this mapping is enabled by default
        /// </summary>
        bool EnabledByDefault { get; }

        /// <summary>
        ///     Gets a value indicating whether or not this mapping can be modified
        /// </summary>
        bool Editable { get; }

        /// <summary>
        ///     Gets the default BetLinePreset for this mapping
        /// </summary>
        int DefaultBetLinePresetId { get; }

        /// <summary>
        ///     Gets the a collection of available BetLinePresets
        /// </summary>
        IEnumerable<int> BetLinePresets { get; }
    }
}