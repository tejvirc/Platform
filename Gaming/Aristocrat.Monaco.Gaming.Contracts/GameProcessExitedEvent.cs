namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An End Game Process event is posted to terminate the current game process.
    /// </summary>
    [Serializable]
    public class GameProcessExitedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameProcessExitedEvent" /> class.
        /// </summary>
        /// <param name="processId">The id of the process that exited</param>
        public GameProcessExitedEvent(int processId)
            : this(processId, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameProcessExitedEvent" /> class.
        /// </summary>
        /// <param name="processId">The id of the process that exited</param>
        /// <param name="unexpected">Indicates whether or not the process exit was unexpected.</param>
        public GameProcessExitedEvent(int processId, bool unexpected)
        {
            ProcessId = processId;
            Unexpected = unexpected;
        }

        /// <summary>
        ///     Gets the process id for the process that exited.
        /// </summary>
        public int ProcessId { get; }

        /// <summary>
        ///     Gets a value indicating whether the process exit was unexpected.
        /// </summary>
        public bool Unexpected { get; }
    }
}