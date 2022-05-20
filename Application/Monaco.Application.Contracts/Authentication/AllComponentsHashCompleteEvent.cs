namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    using System;
    using System.Threading;
    using Kernel;

    /// <summary>
    ///     An event when all component hashes complete
    /// </summary>

    [Serializable]
    public class AllComponentsHashCompleteEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="cancelled">True if hashes were cancelled</param>
        /// <param name="taskToken">Use the group cancellation token as an ID.</param>
        public AllComponentsHashCompleteEvent(bool cancelled, CancellationToken taskToken)
        {
            Cancelled = cancelled;
            TaskToken = taskToken;
        }

        /// <summary>
        ///     True if hashes were cancelled.
        /// </summary>
        public bool Cancelled { get; }

        /// <summary>
        ///     Use the cancellation token as a unique task identifier.
        /// </summary>
        public CancellationToken TaskToken { get; }
    }
}