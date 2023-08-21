namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     A GameConnectedEvent should be posted when IPC connection is made, (join)
    /// </summary>
    [ProtoContract]
    public class GameConnectedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameConnectedEvent" /> class.
        /// </summary>
        /// <param name="isReplay">The game replay status.</param>
        public GameConnectedEvent(bool isReplay)
        {
            IsReplay = isReplay;
        }

        /// <summary>
        /// Parameterless constructor used while deserializing
        /// </summary>
        public GameConnectedEvent()
        {
        }

        /// <summary>
        ///     Gets the replay status of the game.
        /// </summary>
        [ProtoMember(1)]
        public bool IsReplay { get; }
    }
}