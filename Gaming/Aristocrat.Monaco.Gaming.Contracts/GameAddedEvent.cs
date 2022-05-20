namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A Game Added Event is posted when a game is enabled.  This typically occurs when the host remotely configures the
    ///     game play devices on the EGM.
    /// </summary>
    [Serializable]
    public class GameAddedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameAddedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game.</param>
        /// <param name="themeId">The theme id of the game.</param>
        public GameAddedEvent(int gameId, string themeId)
        {
            GameId = gameId;
            ThemeId = themeId;
        }

        /// <summary>
        ///     Gets the unique Game Id
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     Gets the ThemeId for the Game
        /// </summary>
        public string ThemeId { get; }
    }
}