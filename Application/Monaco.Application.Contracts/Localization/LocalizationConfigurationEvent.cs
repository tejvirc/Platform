namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Notification that localization configuration has been loaded.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class LocalizationConfigurationEvent : LocalizationEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationConfigurationEvent"/> class.
        /// </summary>
        /// <param name="configPath">Configuration path.</param>
        /// <param name="overridePaths">Override paths.</param>
        public LocalizationConfigurationEvent(string configPath, string[] overridePaths)
        {
            ConfigPath = configPath;
            OverridePaths = overridePaths;
        }

        /// <summary>
        ///     Gets the configured localization overrides path.
        /// </summary>
        public string ConfigPath { get; }

        /// <summary>
        ///     Gets the localization override paths.
        /// </summary>
        public string[] OverridePaths { get; }
    }
}
