namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A GameConnectedEvent should be posted when IPC connection is made, (join)
    /// </summary>
    [Serializable]
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
        ///     Gets the replay status of the game.
        /// </summary>
        public bool IsReplay { get; }
    }
}