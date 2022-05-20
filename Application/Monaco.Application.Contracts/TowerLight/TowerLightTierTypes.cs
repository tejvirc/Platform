namespace Aristocrat.Monaco.Application.Contracts.TowerLight
{
    using System.ComponentModel;

    /// <summary>
    ///     TowerLight Tower types
    /// </summary>
    public enum TowerLightTierTypes
    {
        /// <summary>Not specified</summary>
        [Description("Undefined")]
        Undefined,

        /// <summary>
        ///     2 Tier light tower
        /// </summary>
        [Description("2-Tier")]
        TwoTier,

        /// <summary>
        ///     4 Tier light tower
        /// </summary>
        [Description("4-Tier")]
        FourTier,
    }
}