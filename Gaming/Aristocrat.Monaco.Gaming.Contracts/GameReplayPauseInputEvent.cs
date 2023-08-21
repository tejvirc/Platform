namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.CodeDom;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Posted to indicate replay pauses until user input.
    /// </summary>
    [ProtoContract]
    public class GameReplayPauseInputEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for derserialization
        /// </summary>
        public GameReplayPauseInputEvent()
        {
        }

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
        [ProtoMember(1)]
        public bool ReplayPauseState { get; set; }
    }
}