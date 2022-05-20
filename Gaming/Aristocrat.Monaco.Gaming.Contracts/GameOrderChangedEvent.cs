namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the GameOrderChangedEvent class.
    /// </summary>
    [Serializable]
    public class GameOrderChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameOrderChangedEvent" /> class.
        /// </summary>
        /// <param name="orderedGameIds">The ordered list of unique identifiers of the game.</param>
        /// <param name="operatorChanged">Operator changed flag.</param>
        public GameOrderChangedEvent(IEnumerable<string> orderedGameIds, bool operatorChanged)
        {
            OrderedGameIds = orderedGameIds;
            GamePosition = -1;
            OperatorChanged = operatorChanged;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameOrderChangedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The game theme identifier for the game to change the order of</param>
        /// <param name="newPosition">The new position for the game</param>
        public GameOrderChangedEvent(string gameId, int newPosition)
        {
            GameId = gameId;
            GamePosition = newPosition;
        }

        /// <summary>
        ///     Gets the ordered list of game theme Ids
        /// </summary>
        public IEnumerable<string> OrderedGameIds { get; }

        /// <summary>
        ///     Gets the game theme identifier
        /// </summary>
        public string GameId { get; }

        /// <summary>
        ///     Gets the new game position
        /// </summary>
        public int GamePosition { get; }

        /// <summary>
        ///     Gets the operator changed.
        /// </summary>
        public bool OperatorChanged { get; }
    }
}