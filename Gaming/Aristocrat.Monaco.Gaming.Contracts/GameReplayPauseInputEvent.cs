namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Posted to indicate replay pauses until user input.
    /// </summary>
    [Serializable]
    public class GameReplayPauseInputEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameReplayPauseInputEvent" /> class.
        /// </summary>
        public GameReplayPauseInputEvent(bool value)
        {
            ReplayPauseState = value;
        }

        /// <summary>
        ///     Property indicating if RuntimeHost is paused during Replay
        /// </summary>
        public bool ReplayPauseState { get; set; }
    }
}