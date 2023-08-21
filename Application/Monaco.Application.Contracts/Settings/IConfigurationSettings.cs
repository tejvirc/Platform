namespace Aristocrat.Monaco.Application.Contracts.Settings
{
    using System.Threading.Tasks;

    /// <summary>
    ///     This interface is used to manage applying and retrieving configuration settings.
    /// </summary>
    public interface IConfigurationSettings
    {
        /// <summary>
        ///     Gets the name of the settings provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets configuration groups that are managed by this configuration settings provider.
        /// </summary>
        ConfigurationGroup Groups { get; }

        /// <summary>
        ///     Initializes the settings provider.
        /// </summary>
        /// <returns><see cref="Task"/>.</returns>
        Task Initialize();

        /// <summary>
        ///     Add settings for configuration group.
        /// </summary>
        /// <param name="configGroup"></param>
        /// <param name="settings"></param>
        /// <returns><see cref="Task"/>.</returns>
        Task Apply(ConfigurationGroup configGroup, object settings);

        /// <summary>
        ///     Gets the configuration settings for specified group.
        /// </summary>
        /// <param name="configGroup">The <see cref="ConfigurationGroup"/>.</param>
        /// <returns>A settings object of configuration settings.</returns>
        /// <returns><see cref="Task"/> A settings object.</returns>
        Task<object> Get(ConfigurationGroup configGroup);
    }
}
