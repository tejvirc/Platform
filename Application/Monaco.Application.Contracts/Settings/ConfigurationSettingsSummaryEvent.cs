namespace Aristocrat.Monaco.Application.Contracts.Settings
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     Published when configuration settings summary is requested. 
    /// </summary>
    public class ConfigurationSettingsSummaryEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigurationSettingsSummaryEvent"/> class.
        /// </summary>
        /// <param name="configGroup"><see cref="ConfigurationGroup"/> configuration group that was exported.</param>
        /// <param name="settings">An object that contains the settings that were exported.</param>
        public ConfigurationSettingsSummaryEvent(ConfigurationGroup configGroup, IReadOnlyDictionary<string, object> settings)
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