namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     The GameDisabledEvent is published when a game is disabled
    /// </summary>
    public class GameDisabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDisabledEvent" /> class.
        /// </summary>
        /// <param name="gameId">The game id</param>
        /// <param name="status">The reason(s) the game was disabled</param>
        /// <param name="themeId">The game theme id</param>
        public GameDisabledEvent(int gameId, GameStatus status, string themeId)
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
        ///     Gets the reason(s) the game was disabled
        /// </summary>
        public GameStatus Status { get; }

        /// <summary>
        ///     Gets the theme id
        /// </summary>
        public string GameThemeId { get; }
    }
}