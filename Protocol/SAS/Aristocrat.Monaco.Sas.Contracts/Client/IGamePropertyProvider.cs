namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    /// Definition of the IGamePropertyProvider interface.
    /// </summary>
    public interface IGamePropertyProvider
    {
        /// <summary>
        /// Gets the number of implemented games in the system.
        /// </summary>
        /// <returns>The number of games implemented.</returns>
        int NumberOfGames
        {
            get;
        }

        /// <summary>
        /// Gets the Game Id for the currently selected game.
        /// </summary>
        int SelectedGameId
        {
            get;
        }

        /// <summary>
        /// Gets the accounting denomination.
        /// </summary>
        ulong AccountingDenomination
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether or not multiple denominations are supported
        /// </summary>
        bool MultipleDenominationsSupported
        {
            get;
        }

        /// <summary>
        /// Returns whether or not the specified sas game number and denomination are enabled.
        /// </summary>
        /// <param name="gameNumber">The Sas specific game number (0-9999)</param>
        /// <param name="denomination">The denomination to check for the game.</param>
        /// <returns>True if the game number is enabled for the specific denomination, 
        /// false otherwise</returns>
        bool IsGameEnabled(int gameNumber, int denomination);

        /// <summary>
        /// Returns the game title id for the specified game number
        /// </summary>
        /// <param name="gameNumber">The game number to get the title id for</param>
        /// <returns>The title id for the specified game</returns>
        int GetGameTitleId(int gameNumber);

        /// <summary>
        /// Returns the game theme for the selected game number.
        /// </summary>
        /// <param name="gameNumber">The game number to get the theme for</param>
        /// <returns>The theme name for the specified game.</returns>
        string GetGameTheme(int gameNumber);

        /// <summary>
        /// Returns the paytable name for the selected game number.
        /// </summary>
        /// <param name="gameNumber">The game number to get the paytable name for</param>
        /// <param name="payTableId">The paytable identifier to get the paytable name for.</param>
        /// <returns>The paytable name for the specified game.</returns>
        string GetPayTableName(int gameNumber, int payTableId);

        /// <summary>
        /// Returns the paytable id for the selected game number.
        /// </summary>
        /// <param name="gameNumber">The game number to get the paytable id for.</param>
        /// <param name="denomination">The Sas game denomination.</param>
        /// <returns>The paytable id for the specified game.</returns>
        int GetPayTableId(int gameNumber, int denomination);

        /// <summary>
        /// Returns the current denomination for the specified game in millicents 
        /// </summary>
        /// <param name="gameNumber">The game to get the denom for.</param>
        /// <returns>The Sas game denomination.</returns>
        ulong GetDenomination(int gameNumber);

        /// <summary>
        /// Get the maximum bet supported by the current game
        /// </summary>
        /// <param name="gameIndex">The index of the game to return the max bet for. 0 returns the 
        /// max bet for the entire cabinet.</param>
        /// <returns>Max bet of the specified game</returns>
        ulong GetBetMax(uint gameIndex);

        /// <summary>
        /// Gets the number of wager categories (bet amounts) for the given game. 
        /// </summary>
        /// <param name="gameIndex">Game Identification Number</param>
        /// <returns>The number of wager categories</returns>
        ulong GetWagerCategories(ulong gameIndex);

        /// <summary>
        /// Gets the denomination for the given wager category and game.
        /// </summary>
        /// <param name="wagerCategory">The wager category</param>
        /// <param name="gameIndex">Game ID number</param>
        /// <returns>The denomination</returns>
        ulong GetWagerCategoryDenom(ulong wagerCategory, ulong gameIndex);

        /// <summary>
        /// Gets the theoretical max payback percent.
        /// </summary>
        /// <param name="gameIndex">Game Identification Number</param>
        /// <returns>The theoretical max payback percent</returns>
        double GetTheoreticalMaxPaybackPercent(uint gameIndex);

        /// <summary>
        /// Gets the theoretical payback percentage for the given denomination and game.  If no specific denomination is
        /// given (or DI_ALL is used), this method calcullates and returns the weighted average theoretical payback percentage
        /// for all supported bet amount s for the given game.
        /// </summary>
        /// <param name="denominationId">Denom ID number</param>
        /// <param name="gameIndex">Game ID number</param>
        /// <returns>payback percentage</returns>
        double GetTheoreticalPaybackPercentage(long denominationId, ulong gameIndex);
    }
}
