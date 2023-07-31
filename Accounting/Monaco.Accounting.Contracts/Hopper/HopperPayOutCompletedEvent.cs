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
        /// <param name="transaction">Coin Out Transaction</param>
        public HopperPayOutCompletedEvent(CoinOutTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets the coin out transaction
        /// </summary>
        public CoinOutTransaction Transaction { get; }
    }
}
