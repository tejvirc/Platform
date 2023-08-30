namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using System;
    using Kernel;

    /// <summary>
    ///     The SideBetChangedEvent is posted when the runtime updates the InSideBet flag.
    /// </summary>
    [Serializable]
    public class SideBetChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SideBetChangedEvent" /> class.
        /// </summary>
        /// <param name="active">The Side Bet window is open</param>
        public SideBetChangedEvent(bool active)
        {
            Active = active;
        }

        /// <summary>
        ///     True if the Side Bet window is open
        /// </summary>
        public bool Active { get; }
    }
}