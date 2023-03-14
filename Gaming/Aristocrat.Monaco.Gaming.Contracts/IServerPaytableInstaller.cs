namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     Installer for paytables that come from a external server
    /// </summary>
    public interface IServerPaytableInstaller
    {
        /// <summary>
        ///     Gets a list of games that are available to have paytables installed
        /// </summary>
        /// <returns>A list of available games</returns>
        IReadOnlyCollection<IGameDetail> GetAvailableGames();

        /// <summary>
        ///     Gets the game for the provided game id
        /// </summary>
        /// <param name="gameId">The game Id to get the paytable for</param>
        /// <returns>The current game details for the provided game ID or null</returns>
        IGameDetail GetGame(int gameId);

        /// <summary>
        ///     Installs the paytables for the provided game ID
        /// </summary>
        /// <param name="gameId">The game ID to install</param>
        /// <param name="paytableConfiguration">The paytable configuration to install</param>
        /// <returns>The current game details for the provided game ID or null</returns>
        IGameDetail InstallGame(int gameId, ServerPaytableConfiguration paytableConfiguration);
    }
}