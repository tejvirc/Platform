namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;
    using ProtoBuf;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the GameIconOrderChangedEvent class.
    /// </summary>
    [Serializable]
    public class GameIconOrderChangedEvent : BaseEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public GameIconOrderChangedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameIconOrderChangedEvent" /> class.
        /// </summary>
        /// <param name="orderedGameIds">The ordered list of unique identifiers of the game.</param>
        /// <param name="operatorChanged">Operator changed flag.</param>
        public GameIconOrderChangedEvent(IEnumerable<string> orderedGameIds, bool operatorChanged)
        {
            OrderedGameIds = orderedGameIds;
            GamePosition = -1;
            OperatorChanged = operatorChanged;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameIconOrderChangedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The game theme identifier for the game to change the order of</param>
        /// <param name="newPosition">The new position for the game</param>
        public GameIconOrderChangedEvent(string gameId, int newPosition)
        {
            GameId = gameId;
            GamePosition = newPosition;
        }

        /// <summary>
        ///     Gets the ordered list of game theme Ids
        /// </summary>
        [ProtoMember(1)]
        public IEnumerable<string> OrderedGameIds { get; }

        /// <summary>
        ///     Gets the game theme identifier
        /// </summary>
        [ProtoMember(2)]
        public string GameId { get; }

        /// <summary>
        ///     Gets the new game position
        /// </summary>
        [ProtoMember(3)]
        public int GamePosition { get; }

        /// <summary>
        ///     Gets the operator changed.
        /// </summary>
        [ProtoMember(4)]
        public bool OperatorChanged { get; }
    }
}