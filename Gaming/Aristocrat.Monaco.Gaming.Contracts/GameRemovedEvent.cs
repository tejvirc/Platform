namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A Game Removed Event is posted when a game is disabled.  This typically occurs when the host remotely configures
    ///     the game play devices on the EGM.
    /// </summary>
    [Serializable]
    public class GameRemovedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameRemovedEvent" /> class.
        /// </summary>
        /// <param name="gameId"> The Id of the game.</param>
        public GameRemovedEvent(int gameId)
        {
            GameId = gameId;
        }

        /// <summary>
        ///     Gets the unique identifier of the game.
        /// </summary>
        public int GameId { get; }
    }
}