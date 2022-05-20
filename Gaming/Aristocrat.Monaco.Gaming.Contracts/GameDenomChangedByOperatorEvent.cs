namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A Game denominations changed event is posted when the active denominations for a game has changed by the operator.
    /// </summary>
    [Serializable]
    public class GameDenomChangedByOperatorEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDenomChangedByOperatorEvent"/> class.
        /// </summary>
        /// <param name="gameId"></param>
        public GameDenomChangedByOperatorEvent(int gameId)
        {
            GameId = gameId;
        }

        /// <summary>
        ///     Gets the Game ID of the target game.
        /// </summary>
        public int GameId { get; }
    }
}
