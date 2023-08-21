namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>A bit-field of flags for specifying import machine setting states.</summary>
    [Flags]
    public enum ImportMachineSettings
    {
        /// <summary>
        ///     None
        /// </summary>
        None = 0x0000,

        /// <summary>
        ///     Imported
        /// </summary>
        Imported = 0x0001,

        /// <summary>
        ///     Config wizard configuration properties loaded
        /// </summary>
        ConfigWizardConfigurationPropertiesLoaded = 0x0002,

        /// <summary>
        ///     Accounting properties loaded
        /// </summary>
        AccountingPropertiesLoaded = 0x0004,

        /// <summary>
        ///     Handpay properties loaded
        /// </summary>
        HandpayPropertiesLoaded = 0x0008,

        /// <summary>
        ///     Application configuration properties loaded
        /// </summary>
        ApplicationConfigurationPropertiesLoaded = 0x0010,

        /// <summary>
        ///     Gaming properties loaded
        /// </summary>
        CabinetFeaturesPropertiesLoaded = 0x0020,

        /// <summary>
        ///     Gaming properties loaded
        /// </summary>
        GamingPropertiesLoaded = 0x0040

        // *NOTE* If adding a flag for when a property provider is loaded to this enumeration, see usage in <see cref="ApplicationRunnable"/>
        // to insure logic that manages a reboot when machine settings have been imported.
    }
}