namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    /// <summary>
    ///     Notification that localization configuration has been loaded.
    /// </summary>
    public class LocalizationConfigurationEvent : LocalizationEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationConfigurationEvent"/> class.
        /// </summary>
        /// <param name="configPath">Configuration path.</param>
        /// <param name="overrideKeys">Override keys.</param>
        public LocalizationConfigurationEvent(string configPath, string[] overrideKeys)
        {
            ConfigPath = configPath;
            OverrideKeys = overrideKeys;
        }

        /// <summary>
        ///     Gets the configured localization overrides path.
        /// </summary>
        public string ConfigPath { get; }

        /// <summary>
        ///     Gets the localization override keys.
        /// </summary>
        public string[] OverrideKeys { get; }
    }
}
