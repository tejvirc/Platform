namespace Aristocrat.Monaco.Application.Contracts.Settings
{
    using System;

    /// <summary>
    ///     Configuration group
    /// </summary>
    [Flags]
    public enum ConfigurationGroup
    {
        /// None
        None = 0x00,

        /// Machine configuration
        Machine = 0x01,

        /// Game configuration
        Game = 0x02
    }
}
