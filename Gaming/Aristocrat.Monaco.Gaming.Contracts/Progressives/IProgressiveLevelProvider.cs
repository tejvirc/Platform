namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System.Collections.Generic;
    using PackageManifest.Models;

    /// <summary>
    ///     The main API for handling progressive levels in the Monaco platform. It provides an interface to the underlying
    ///     progressive level systems.
    /// </summary>
    public interface IProgressiveLevelProvider
    {
        /// <summary>
        ///     Loads progressive levels from the progressive detail data when games are loaded.
        /// </summary>
        /// <param name="gameDetails">The details for a given game</param>
        /// <param name="progressiveDetails">The progressive details for a given game</param>
        void LoadProgressiveLevels(IGameDetail gameDetails,
            IEnumerable<ProgressiveDetail> progressiveDetails);

        /// <summary>
        ///     Gets the current list of progressive levels
        /// </summary>
        /// <returns>the current list of levels</returns>
        IReadOnlyCollection<ProgressiveLevel> GetProgressiveLevels();

        /// <summary>
        ///     Gets a filtered list of levels
        /// </summary>
        /// <param name="packName">The progressive pack name</param>
        /// <param name="gameId">The game identifier</param>
        /// <param name="denomination">The denomination</param>
        /// <param name="wagerCredits">Wager Credits</param>
        /// <returns>the current list of levels</returns>
        IReadOnlyCollection<ProgressiveLevel> GetProgressiveLevels(string packName, int gameId, long denomination, long wagerCredits = 0);

        /// <summary>
        ///     Updates the provided list of progressive levels
        /// </summary>
        /// <param name="levelUpdates">The level updates to make for each the pack, game, and denom</param>
        void UpdateProgressiveLevels(
            IEnumerable<(string packName,
                int gameId,
                long denomination,
                IEnumerable<ProgressiveLevel> levels)> levelUpdates);

        /// <summary>
        ///     Updates the provided list of progressive levels
        /// </summary>
        /// <param name="packName">The progressive pack name</param>
        /// <param name="gameId">The game identifier</param>
        /// <param name="denomination">The denomination</param>
        /// <param name="levels">The collection of levels to update</param>
        /// <returns>the current list of levels</returns>
        void UpdateProgressiveLevels(
            string packName,
            int gameId,
            long denomination,
            IEnumerable<ProgressiveLevel> levels);
    }
}