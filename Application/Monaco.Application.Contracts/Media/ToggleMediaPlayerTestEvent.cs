namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;
    using ProtoBuf;
    using System;

    /// <summary>
    /// The <see cref="ToggleMediaPlayerTestEvent"/> is triggered when a toggle media player test event occurs in the TestToolView
    /// </summary>
    [ProtoContract]
    public class ToggleMediaPlayerTestEvent : BaseEvent
    {
        /// <summary>
        ///     Media Player ID
        /// </summary>
        [ProtoMember(1)]
        public int PlayerId { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToggleMediaPlayerTestEvent" /> class
        /// </summary>
        /// <param name="playerId">Media Player ID</param>
        public ToggleMediaPlayerTestEvent(int playerId)
        {
            PlayerId = playerId;
        }

        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public ToggleMediaPlayerTestEvent()
        {
        }

    }
}
