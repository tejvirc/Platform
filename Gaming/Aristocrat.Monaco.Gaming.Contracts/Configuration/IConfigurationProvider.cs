namespace Aristocrat.Monaco.Gaming.Contracts.Configuration
{
    using System.Collections.Generic;
    using Kernel;
    using PackageManifest.Models;

    /// <summary>
    ///     Provides a mechanism to edit game configuration
    /// </summary>
    public interface IConfigurationProvider : IService
    {
        /// <summary>
        ///     Gets the set of configuration restrictions by theme id.
        /// </summary>
        /// <param name="themeId">Game theme id.</param>
        /// <returns>IEnumerable&lt;IConfigurationRestriction&gt;.</returns>
        IEnumerable<IConfigurationRestriction> GetByThemeId(string themeId);

        /// <summary>
        ///     Get the default Configuration Restriction for the given ThemeId, if available.
        /// </summary>
        /// <param name="themeId">The theme identifier.</param>
        /// <returns>The default Configuration Restriction, otherwise returns <c>null</c></returns>
        IConfigurationRestriction GetDefaultByThemeId(string themeId);

        /// <summary>
        ///     Loads the configurations used to provide the restrictions
        /// </summary>
        /// <param name="gameThemeId">The game theme ID.</param>
        /// <param name="configurations">The configurations.</param>
        /// <param name="defaultConfiguration">The index of default configuration.</param>
        void Load(string gameThemeId, IEnumerable<Configuration> configurations, Configuration defaultConfiguration = null);
    }
}