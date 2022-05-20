namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;
    using System;

    /// <summary>
    /// The <see cref="ToggleMediaPlayerTestEvent"/> is triggered when a toggle media player test event occurs in the TestToolView
    /// </summary>
    [Serializable]
    public class ToggleMediaPlayerTestEvent : BaseEvent
    {
        /// <summary>
        ///     Media Player ID
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToggleMediaPlayerTestEvent" /> class
        /// </summary>
        /// <param name="playerId">Media Player ID</param>
        public ToggleMediaPlayerTestEvent(int playerId)
        {
            PlayerId = playerId;
        }
    }
}
