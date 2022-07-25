namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;
    using Aristocrat.PackageManifest.Extension.v100;
    using Models;
    using PackageManifest.Models;

    /// <summary>
    ///     Provides a mechanism to retrieve and interact with the available games.
    /// </summary>
    public interface IGameProvider
    {
        /// <summary>
        ///     Gets a game by its id
        /// </summary>
        /// <param name="gameId">The game id</param>
        /// <returns>An <see cref="IGameDetail" /> if found, else null</returns>
        IGameDetail GetGame(int gameId);

        /// <summary>
        ///     Gets a game and denomination by its id
        /// </summary>
        /// <param name="gameId">The game id</param>
        /// <param name="denomination">The denomination</param>
        /// <returns>An <see cref="IGameDetail" /> and <see cref="IDenomination"/> if found, else null</returns>
        (IGameDetail game, IDenomination denomination) GetGame(int gameId, long denomination);

        /// <summary>
        ///     Gets a collection of active games
        /// </summary>
        /// <returns>A collection of games</returns>
        IReadOnlyCollection<IGameDetail> GetGames();

        /// <summary>
        ///     Gets a collection of enabled games
        /// </summary>
        /// <returns>A collection of games</returns>
        IReadOnlyCollection<IGameDetail> GetEnabledGames();

        /// <summary>
        ///     Gets a collection of active and inactive games
        /// </summary>
        /// <returns>A collection of games</returns>
        IReadOnlyCollection<IGameDetail> GetAllGames();

        /// <summary>
        ///     Gets a collection of game combos
        /// </summary>
        /// <returns>A collection of game combos</returns>
        IReadOnlyCollection<IGameCombo> GetGameCombos();

        /// <summary>
        ///     Get the active game and denomination
        /// </summary>
        /// <returns>Active game and denomination</returns>
        (IGameDetail game, IDenomination denomination) GetActiveGame();

        /// <summary>
        ///     Gets the minimum number of mechanical reels needed to run a game
        /// </summary>
        /// <returns>The minimum number of mechanical reels</returns>
        int GetMinimumNumberOfMechanicalReels();

        /// <summary>
        ///     Verifies that the game configuration is valid
        /// </summary>
        /// <param name="gameProfile">A game profile instance</param>
        /// <returns>true if the configuration is valid</returns>
        bool ValidateConfiguration(IGameDetail gameProfile);

        /// <summary>
        ///     Verifies that the game configuration is valid
        /// </summary>
        /// <param name="gameProfile">A game profile instance</param>
        /// <param name="denominations">The list of denominations to be activated</param>
        /// <returns>true if the configuration is valid</returns>
        bool ValidateConfiguration(IGameDetail gameProfile, IEnumerable<long> denominations);

        /// <summary>
        ///     Enable a game with the provided status
        /// </summary>
        /// <param name="gameId">The game id</param>
        /// <param name="status">The reason the game is being disabled.</param>
        void EnableGame(int gameId, GameStatus status);

        /// <summary>
        ///     Disables a game with the provided status
        /// </summary>
        /// <param name="gameId">The game id</param>
        /// <param name="status">The reason the game is being disabled.</param>
        void DisableGame(int gameId, GameStatus status);

        /// <summary>
        ///     Sets the list of active denominations for a game
        /// </summary>
        /// <param name="gameId">The game id</param>
        /// <param name="denominations">The list of active denominations</param>
        void SetActiveDenominations(int gameId, IEnumerable<long> denominations);

        /// <summary>
        ///     Sets the list of active denominations for a game
        /// </summary>
        /// <param name="gameId">The game id</param>
        /// <param name="denominations">The list of active denominations</param>
        void SetActiveDenominations(int gameId, IEnumerable<IDenomination> denominations);

        /// <summary>
        ///     Adds a games located in the specified path
        /// </summary>
        /// <param name="path">The path to the game</param>
        /// <returns>true upon success, otherwise false</returns>
        bool Add(string path);

        /// <summary>
        ///     Removes games located in the specified path
        /// </summary>
        /// <param name="path">The path to the game</param>
        /// <returns>true upon success, otherwise false</returns>
        bool Remove(string path);

        /// <summary>
        ///     Replace the specified game with the game located in the specified path
        /// </summary>
        /// <param name="path">The path to the game</param>
        /// <param name="game">The game detail</param>
        /// <returns>true upon success, otherwise false</returns>
        bool Replace(string path, IGameDetail game);

        /// <summary>
        ///     Registers a game located at the specified path
        /// </summary>
        /// <param name="path">The path to the game</param>
        /// <returns>true upon success, otherwise false</returns>
        bool Register(string path);

        /// <summary>
        ///     Uses the manifest at the specified path to check for an existing game with the same details
        /// </summary>
        /// <param name="path">The path to the game</param>
        /// <returns>The game profile of the existing device if one exists</returns>
        IGameDetail Exists(string path);

        /// <summary>
        ///     Configures the game with the provided values.
        /// </summary>
        /// <param name="gameId">The game identifier.</param>
        /// <param name="gameOptionConfigValues">The game option configuration values.</param>
        void Configure(int gameId, GameOptionConfigValues gameOptionConfigValues);

        /// <summary>
        ///     Sets the list of metadata tag strings to the game
        /// </summary>
        /// <param name="gameId">The game identifier.</param>
        /// <param name="tags">The tag string to add to the game</param>
        void SetGameTags(int gameId, IEnumerable<string> tags);

        /// <summary>
        ///     Updates game runtime targets after runtime instance is removed with the installer.
        /// </summary>
        void UpdateGameRuntimeTargets();

        /// <summary>
        ///     Return whether or not total RTP includes progressive increment contribution.
        /// </summary>
        /// <param name="type">Game type</param>
        /// <returns>Whether or not total RTP includes progressive increment contribution.</returns>
        bool CanIncludeIncrementRtp(GameType type);

        /// <summary>
        ///     Return total RTP range, calculated per jurisdiction rules.
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="progressiveDetails">Progressive info</param>
        /// <returns>Total RTP range</returns>
        RtpRange GetTotalRtp(GameAttributes game, IReadOnlyCollection<ProgressiveDetail> progressiveDetails);

        /// <summary>
        ///     Return whether or not total RTP is valid
        /// </summary>
        /// <param name="gameType">Game type</param>
        /// <param name="rtpRange">RTP range</param>
        /// <returns>Whether or not total RTP range is valid</returns>
        bool IsValidRtp(GameType gameType, RtpRange rtpRange);
    }
}