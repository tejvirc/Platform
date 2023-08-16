namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Indicates the game play state has changed. This event is trigger whenever the current game play state transitions
    ///     to a new state.
    /// </summary>
    [ProtoContract]
    public class GamePlayStateChangedEvent : BaseEvent
    {
        /// <summary>
        /// Add empty constructor for serialization and deserialization
        /// </summary>
        public GamePlayStateChangedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayStateChangedEvent" /> class.
        /// </summary>
        /// <param name="previous">The previous state.</param>
        /// <param name="current">The current state.</param>
        public GamePlayStateChangedEvent(PlayState previous, PlayState current)
        {
            PreviousState = previous;
            CurrentState = current;
        }

        /// <summary>
        ///     Gets the previous state.
        /// </summary>
        [ProtoMember(1)]
        public PlayState PreviousState { get; }

        /// <summary>
        ///     Gets the current state.
        /// </summary>
        [ProtoMember(2)]
        public PlayState CurrentState { get; }
    }
}