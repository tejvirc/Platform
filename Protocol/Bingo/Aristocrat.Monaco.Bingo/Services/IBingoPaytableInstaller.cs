namespace Aristocrat.Monaco.Bingo.Services
{
    using System.Collections.Generic;
    using Common.Storage.Model;
    using Configuration;

    /// <summary>
    ///     The configuration provider for games for Bingo
    /// </summary>
    public interface IBingoPaytableInstaller
    {
        /// <summary>
        ///     Configures the games on the EGM.  This will install any new paytables that didn't exist before
        /// </summary>
        /// <param name="gameConfigurations">The configurations to configure</param>
        /// <returns>A list of the configured games</returns>
        IEnumerable<BingoGameConfiguration> ConfigureGames(IEnumerable<ServerGameConfiguration> gameConfigurations);

        /// <summary>
        ///     Updates the configuration data from the server
        /// </summary>
        /// <param name="gameConfigurations">The configurations to update</param>
        /// <returns>A list of the configured games</returns>
        IEnumerable<BingoGameConfiguration> UpdateConfiguration(
            IEnumerable<ServerGameConfiguration> gameConfigurations);

        /// <summary>
        ///     Checks if the provided game configuration is valid for this EGM
        /// </summary>
        /// <param name="gameConfigurations">The game configurations to check</param>
        /// <returns>Whether or not the game configurations are valid</returns>
        bool IsConfigurationValid(IEnumerable<ServerGameConfiguration> gameConfigurations);
    }
}