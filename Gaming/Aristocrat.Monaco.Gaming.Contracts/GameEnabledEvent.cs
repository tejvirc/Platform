namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     The GameEnabledEvent is published when a game is enabled
    /// </summary>
    public class GameEnabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEnabledEvent" /> class.
        /// </summary>
        /// <param name="gameId">The game id</param>
        /// <param name="status">The reason(s) the game was enabled</param>
        /// <param name="themeId">The game theme id</param>
        public GameEnabledEvent(int gameId, GameStatus status, string themeId)
        {
            GameId = gameId;
            Status = status;
            GameThemeId = themeId;
        }

        /// <summary>
        ///     Gets the game id
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     Gets the reason(s) the game was enabled
        /// </summary>
        public GameStatus Status { get; }

        /// <summary>
        ///     Gets the theme id
        /// </summary>
        public string GameThemeId { get; }
    }
}