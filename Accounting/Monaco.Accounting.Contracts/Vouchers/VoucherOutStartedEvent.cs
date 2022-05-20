namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Event emitted when a VoucherOut has started.
    /// </summary>
    [Serializable]
    public class VoucherOutStartedEvent : BaseEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="amount"></param>
        public VoucherOutStartedEvent(long amount)
        {
            Amount = amount;
        }
        /// <summary>
        /// Amount
        /// </summary>
        public long Amount { get; }
    }
}