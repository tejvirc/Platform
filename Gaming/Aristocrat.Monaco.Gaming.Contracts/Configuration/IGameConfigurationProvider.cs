namespace Aristocrat.Monaco.Gaming.Contracts.Configuration
{
    using Kernel;

    /// <summary>
    ///     Provides a mechanism to view and edit game configurations
    /// </summary>
    public interface IGameConfigurationProvider : IService
    {
        /// <summary>
        ///     Gets the configuration restriction for a given themeId
        /// </summary>
        /// <param name="themeId">The theme identifier</param>
        /// <returns>The configuration restriction, if present</returns>
        IConfigurationRestriction GetActive(string themeId);

        /// <summary>
        ///     Applies the configuration
        /// </summary>
        /// <param name="themeId">The theme identifier</param>
        /// <param name="restriction">The base configuration restriction</param>
        /// <returns>true if successful, otherwise false</returns>
        void Apply(string themeId, IConfigurationRestriction restriction);
    }
}