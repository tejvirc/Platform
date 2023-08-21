namespace Aristocrat.Monaco.Application.Contracts.Settings
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     Published when configuration settings are imported.
    /// </summary>
    public class ConfigurationSettingsImportedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigurationSettingsImportedEvent"/> class.
        /// </summary>
        /// <param name="configGroup"><see cref="ConfigurationGroup"/> configuration group that was imported.</param>
        /// <param name="settings">An object that contains the settings that were imported.</param>
        public ConfigurationSettingsImportedEvent(ConfigurationGroup configGroup, IReadOnlyDictionary<string, object> settings)
        {
            Group = configGroup;
            Settings = settings;
        }

        /// <summary>
        ///     Gets the configuration group.
        /// </summary>
        public ConfigurationGroup Group { get; }

        /// <summary>
        ///     Gets the settings object.
        /// </summary>
        public IReadOnlyDictionary<string, object> Settings { get; }
    }
}
