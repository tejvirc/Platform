namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A Game Process Hung event is posted to when the game/runtime is no longer responding
    /// </summary>
    [Serializable]
    public class GameProcessHungEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameProcessHungEvent" /> class.
        /// </summary>
        /// <param name="processId">The Id of the hung process</param>
        public GameProcessHungEvent(int processId)
        {
            ProcessId = processId;
        }

        /// <summary>
        ///     Gets the id of the hung process
        /// </summary>
        public int ProcessId { get; }
    }
}