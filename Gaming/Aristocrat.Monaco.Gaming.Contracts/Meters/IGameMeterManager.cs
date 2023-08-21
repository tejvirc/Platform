namespace Aristocrat.Monaco.Gaming.Contracts.Meters
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Kernel;

    /// <summary>
    ///     Definition of the IGameMeterManager interface.
    /// </summary>
    public interface IGameMeterManager : IService
    {
        /// <summary>
        ///     Gets a count of games being represented in the game meter manager interface.
        /// </summary>
        int GameCount { get; }

        /// <summary>
        ///     Gets a count of games currently being stored in the game meter manager interface.
        /// </summary>
        int GameIdCount { get; }

        /// <summary>
        ///     Gets the total number of denoms being represented in the game meter manager interface.
        /// </summary>
        int DenominationCount { get; }

        /// <summary>
        ///     Gets the total number of wager categories being represented in the game meter manager interface.
        /// </summary>
        int WagerCategoryCount { get; }

        /// <summary>
        ///     Registers a <see cref="GameAdded" /> event handler.
        /// </summary>
        event EventHandler<GameAddedEventArgs> GameAdded;

        /// <summary>
        ///     Adds a new set of game meters to the game meter manager, if it is possible.
        /// </summary>
        /// <param name="game">The game being added.</param>
        /// <returns>True if successful, false otherwise. </returns>
        bool AddGame(IGameDetail game);

        /// <summary>
        ///     Adds a new set of game meters to the game meter manager, if it is possible.
        /// </summary>
        /// <param name="games">The games being added.</param>
        void AddGames(IReadOnlyCollection<IGameDetail> games);

        /// <summary>
        ///     Gets the storage block index for a game.
        /// </summary>
        /// <param name="gameId">The game identifier.</param>
        /// <returns>The block index</returns>
        int GetBlockIndex(int gameId);

        /// <summary>
        ///     Gets the storage block index for a game.
        /// </summary>
        /// <param name="gameId">The game identifier.</param>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <returns>The block index</returns>
        int GetBlockIndex(int gameId, long betAmount);

        /// <summary>
        ///     Gets the storage block index for a game.
        /// </summary>
        /// <param name="gameId">The game identifier.</param>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="wagerCategory">The wager category for the game.</param>
        /// <returns>The block index</returns>
        int GetBlockIndex(int gameId, long betAmount, string wagerCategory);

        /// <summary>
        ///     Gets the meter for the provided name.
        /// </summary>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(string meterName);

        /// <summary>
        ///     Gets the named meter for the given game.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(int gameId, string meterName);

        /// <summary>
        ///     Gets the named meter for the given game.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(int gameId, long betAmount, string meterName);

        /// <summary>
        ///     Gets the named meter for the given game.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="wagerCategory">The wager category for the game.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(int gameId, string wagerCategory, string meterName);

        /// <summary>
        ///     Gets the named meter for the given name
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="wagerCategory">The wager category for the game.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(int gameId, long betAmount, string wagerCategory, string meterName);

        /// <summary>
        ///     Gets the named meter for the given game.
        /// </summary>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(long betAmount, string meterName);

        /// <summary>
        ///     Gets the formatted meter name.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>A formatted meter name. </returns>
        string GetMeterName(int gameId, string meterName);

        /// <summary>
        ///     Gets the formatted meter name.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained. </param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>A formatted meter name. </returns>
        string GetMeterName(int gameId, long betAmount, string meterName);

        /// <summary>
        ///     Gets the named meter for the given game.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="wagerCategory">The wager category for the game.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        string GetMeterName(int gameId, string wagerCategory, string meterName);

        /// <summary>
        ///     Gets the named meter for the given game.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="wagerCategory">The wager category for the game.</param>
        /// <param name="meterName">The name of the meter.</param>
        /// <returns>An IMeter object representing that meter.</returns>
        string GetMeterName(int gameId, long betAmount, string wagerCategory, string meterName);

        /// <summary>
        ///     Gets the formatted meter name.
        /// </summary>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained. </param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>A formatted meter name. </returns>
        string GetMeterName(long betAmount, string meterName);

        /// <summary>
        ///     Returns whether or not a meter with the given name is provided.
        /// </summary>
        /// <param name="meterName">The name of the meter in question</param>
        /// <returns>A value indicating whether or not a meter with the given name is provided</returns>
        bool IsMeterProvided(string meterName);

        /// <summary>
        ///     Returns whether or not a meter with the given name is provided.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="meterName">The name of the meter in question</param>
        /// <returns>A value indicating whether or not a meter with the given name is provided</returns>
        bool IsMeterProvided(int gameId, string meterName);

        /// <summary>
        ///     Returns whether or not a meter with the given name is provided.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained. </param>
        /// <param name="meterName">The name of the meter in question</param>
        /// <returns>A value indicating whether or not a meter with the given name is provided</returns>
        bool IsMeterProvided(int gameId, long betAmount, string meterName);

        /// <summary>
        ///     Gets the named meter for the given game.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="wagerCategory">The wager category for the game.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        bool IsMeterProvided(int gameId, string wagerCategory, string meterName);

        /// <summary>
        ///     Gets the named meter for the given game.
        /// </summary>
        /// <param name="gameId">The game identifier. </param>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="wagerCategory">The wager category for the game.</param>
        /// <param name="meterName">The name of the meter.</param>
        /// <returns>An IMeter object representing that meter.</returns>
        bool IsMeterProvided(int gameId, long betAmount, string wagerCategory, string meterName);

        /// <summary>
        ///     Returns whether or not a meter with the given name is provided.
        /// </summary>
        /// <param name="betAmount">The bet amount (in credits) for which the associated meter is to be obtained. </param>
        /// <param name="meterName">The name of the meter in question</param>
        /// <returns>A value indicating whether or not a meter with the given name is provided</returns>
        bool IsMeterProvided(long betAmount, string meterName);
    }
}