namespace Aristocrat.Monaco.Accounting.Contracts.Hopper
{
    using System;
    using Kernel;

    /// <summary>
    ///     Event emitted when a Hopper pay Out has completed.
    /// </summary>
    [Serializable]
    public class HopperPayOutCompletedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HopperPayOutCompletedEvent" /> class.
        /// </summary>
        /// <param name="amount">Transferred amount</param>
        public HopperPayOutCompletedEvent(long amount)
        {
            Amount = amount;
        }

        /// <summary>
        ///     Gets the transferred amount
        /// </summary>
        public long Amount { get; }
    }
}
