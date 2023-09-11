namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Definition of a game.  It's defined by the theme, paytable, and a list of denominations.
    /// </summary>
    public interface IGame : IReleaseInfo
    {
        /// <summary>
        ///     Gets the unique identifier of the game.
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     Gets the theme of the game.
        /// </summary>
        string ThemeId { get; }

        /// <summary>
        ///     Gets the algorithm used to determine the payouts from the game.
        /// </summary>
        string PaytableId { get; }

        /// <summary>
        ///     Gets a value indicating whether or not the game is active. An inactive game has been removed from the system and is
        ///     no longer available for play.
        /// </summary>
        bool Active { get; }
    }
}