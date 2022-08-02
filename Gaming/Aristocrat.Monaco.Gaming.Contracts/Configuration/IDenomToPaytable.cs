namespace Aristocrat.Monaco.Gaming.Contracts.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to map a denomination to a paytable
    /// </summary>
    public interface IDenomToPaytable
    {
        /// <summary>
        ///     Gets or sets a value indicating whether this mapping is active. Inactive mappings
        ///     will not be available for play so can be hidden in UI.
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
        ///     Gets the a collection of available BetLinePresets for this denomination
        /// </summary>
        IEnumerable<int> BetLinePresets { get; }
    }
}