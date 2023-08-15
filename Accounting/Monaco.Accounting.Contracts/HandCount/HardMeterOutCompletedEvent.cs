namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using System;
    using Kernel;

    /// <summary>
    ///     Event emitted when a Hard Meter Out has completed.
    /// </summary>
    [Serializable]
    public class HardMeterOutCompletedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardMeterOutCompletedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        public HardMeterOutCompletedEvent(HardMeterOutTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets the associated transaction
        /// </summary>
        public HardMeterOutTransaction Transaction { get; }
    }
}