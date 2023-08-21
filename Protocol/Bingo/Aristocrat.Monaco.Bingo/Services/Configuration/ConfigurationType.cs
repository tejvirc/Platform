namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    /// <summary>
    ///     Defines the different configuration types received from the Bingo server
    /// </summary>
    public enum ConfigurationType
    {
        /// <summary>
        ///     Defines machine and game configuration settings
        /// </summary>
        MachineAndGameConfiguration,

        /// <summary>
        ///     Defines compliance configuration settings
        /// </summary>
        ComplianceConfiguration,

        /// <summary>
        ///     Defines system configuration settings
        /// </summary>
        SystemConfiguration,

        /// <summary>
        ///     Defines message configuration settings
        /// </summary>
        MessageConfiguration
    }
}
