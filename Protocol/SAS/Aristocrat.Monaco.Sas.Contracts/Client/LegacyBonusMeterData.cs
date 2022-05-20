namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    /// Small class for holding Legacy Bonus meter data.
    /// </summary>
    public class LegacyBonusMeterData
    {
        /// <summary>
        /// Gets or sets the Deductible meter value.
        /// </summary>
        public uint Deductible { get; set; }

        /// <summary>
        /// Gets or sets the Nondeductible meter value.
        /// </summary>
        public uint Nondeductible { get; set; }

        /// <summary>
        /// Gets or sets the WagerMatch meter value.
        /// </summary>
        public uint WagerMatch { get; set; }
    }
}
