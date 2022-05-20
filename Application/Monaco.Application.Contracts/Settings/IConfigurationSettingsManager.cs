namespace Aristocrat.Monaco.Application.Contracts.Settings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    ///     Used for managing import and export of machine settings.
    /// </summary>
    public interface IConfigurationSettingsManager
    {
        /// <summary>
        ///     Gets a preview of the settings from the storage device
        /// </summary>
        /// <param name="configGroup">Configuration group.</param>
        /// <param name="providerName">An optional provider</param>
        /// <returns>A collection of configurations</returns>
        IEnumerable<(ConfigurationGroup group, IDictionary<string, object> settings)> Preview(
            ConfigurationGroup configGroup,
            string providerName = null);

        /// <summary>
        ///     Imports machine settings fom the previewed settings
        /// </summary>
        /// <param name="group">The group for the provided settings</param>
        /// <param name="settings">The configuration settings to import.  These will be settings from the preview</param>
        /// <param name="providerName">An optional provider</param>
        /// <returns><see cref="Task"/></returns>
        Task Import(
            ConfigurationGroup group,
            IReadOnlyDictionary<string, object> settings,
            string providerName = null);

        /// <summary>
        ///     Import machine settings from storage device.
        /// </summary>
        /// <param name="configGroup"></param>
        /// <param name="providerName"></param>
        /// <returns><see cref="Task"/>.</returns>
        Task Import(ConfigurationGroup configGroup, string providerName = null);

        /// <summary>
        ///     Export machine settings to storage device.
        /// </summary>
        /// <param name="configGroup">Configuration group.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task Export(ConfigurationGroup configGroup);

        /// <summary>
        ///     Request Summary for Machine Settings.
        /// </summary>
        /// <param name="configGroup">Configuration group.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task Summary(ConfigurationGroup configGroup);

        /// <summary>
        ///     Request to check of a configuration file is present. 
        /// </summary>
        /// <param name="configGroup">Configuration group.</param>
        /// <returns>true: is config file exist on the drive or else.</returns>
        bool IsConfigurationImportFilePresent(ConfigurationGroup configGroup);
    }
}
