namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     The definition of a game combination, often referred to as a game combo, is the joining of one theme, one paytable,
    ///     and one denomination to result in a game that is accessible to a player.
    /// </summary>
    public interface IGameCombo
    {
        /// <summary>
        ///     Gets the unique identifier of the game combo.
        /// </summary>
        long Id { get; }

        /// <summary>
        ///     Gets the identifier of the game.
        /// </summary>
        int GameId { get; }

        /// <summary>
        ///     Gets the theme of the game.
        /// </summary>
        string ThemeId { get; }

        /// <summary>
        ///     Gets the algorithm used to determine the payouts from the game.
        /// </summary>
        string PaytableId { get; }

        /// <summary>
        ///     Gets the value of each credit wagered as part of the game
        /// </summary>
        long Denomination { get; }

        /// <summary>
        ///     Gets the bet option of the game
        /// </summary>
        string BetOption { get; }
    }
}