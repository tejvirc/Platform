namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     The AwaitingPlayerSelectionChangedEvent is posted when the runtime updates the Awaiting Player Selection flag.
    /// </summary>
    [Serializable]
    public class AwaitingPlayerSelectionChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AwaitingPlayerSelectionChangedEvent" /> class.
        /// </summary>
        /// <param name="awaitingPlayerSelection">Awaiting Player Selection</param>
        public AwaitingPlayerSelectionChangedEvent(bool awaitingPlayerSelection)
        {
            AwaitingPlayerSelection = awaitingPlayerSelection;
        }

        /// <summary>
        ///     Gets the AwaitingPlayerSelection parameter
        /// </summary>
        public bool AwaitingPlayerSelection { get; }
    }
}