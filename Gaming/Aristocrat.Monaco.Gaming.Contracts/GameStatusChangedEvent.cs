namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     GameStatusChangedEvent is published when the Game Profile's disabled Status changes
    /// </summary>
    public class GameStatusChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameStatusChangedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The game id</param>
        public GameStatusChangedEvent(int gameId)
        {
            GameId = gameId;
        }

        /// <summary>
        ///     Gets the game id
        /// </summary>
        public int GameId { get; }
    }
}
