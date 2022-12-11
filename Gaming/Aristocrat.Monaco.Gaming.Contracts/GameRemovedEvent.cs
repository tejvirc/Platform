namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     A Game Removed Event is posted when a game is disabled.  This typically occurs when the host remotely configures
    ///     the game play devices on the EGM.
    /// </summary>
    [ProtoContract]
    public class GameRemovedEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public GameRemovedEvent()
        {
        }

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
        [ProtoMember(1)]
        public int GameId { get; }
    }
}