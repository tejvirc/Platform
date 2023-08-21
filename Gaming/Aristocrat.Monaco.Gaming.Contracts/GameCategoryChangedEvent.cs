namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;
    using Models;

    /// <summary>
    ///     The GameCategoryChangedEvent is published when a game category setting has changed
    /// </summary>
    public class GameCategoryChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameCategoryChangedEvent" /> class.
        /// </summary>
        /// <param name="gameType">The game id</param>
        public GameCategoryChangedEvent(GameType gameType)
        {
            GameType = gameType;
        }

        /// <summary>
        ///     Gets the game category type
        /// </summary>
        public GameType GameType { get; }
    }
}