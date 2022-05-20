namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Indicates the game play state has changed. This event is trigger whenever the current game play state transitions
    ///     to a new state.
    /// </summary>
    [Serializable]
    public class GamePlayStateChangedEvent : BaseEvent
    {
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
        public PlayState PreviousState { get; }

        /// <summary>
        ///     Gets the current state.
        /// </summary>
        public PlayState CurrentState { get; }
    }
}