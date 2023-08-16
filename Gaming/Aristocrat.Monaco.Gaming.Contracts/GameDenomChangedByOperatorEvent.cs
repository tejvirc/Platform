namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     A Game denominations changed event is posted when the active denominations for a game has changed by the operator.
    /// </summary>
    [ProtoContract]
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
        /// Parameterless constructor used while deserializing
        /// </summary>
        public GameDenomChangedByOperatorEvent()
        {
        }

        /// <summary>
        ///     Gets the Game ID of the target game.
        /// </summary>
        [ProtoMember(1)]
        public int GameId { get; }
    }
}
