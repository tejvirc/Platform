namespace Aristocrat.Monaco.Gaming.UI.Events
{
    using System;
    using Contracts.Models;
    using Kernel;

    [Serializable]

    public class LobbyStateChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Gets the previous state.
        /// </summary>
        public LobbyState PreviousState { get; }

        /// <summary>
        ///     Gets the current state.
        /// </summary>
        public LobbyState CurrentState { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LobbyStateChangedEvent" /> class.
        /// </summary>
        /// <param name="previous">The previous state.</param>
        /// <param name="current">The current state.</param>
        public LobbyStateChangedEvent(LobbyState previousState, LobbyState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }
}


