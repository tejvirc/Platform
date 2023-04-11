namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    /// 
    /// </summary>
    public class HandCountCashoutEvent: BaseEvent
    {
        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        public long CashableAmount { get; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="amount">True if overlay window is visible</param>
        public HandCountCashoutEvent(long amount)
        {
            CashableAmount = amount;
        }
    }
}
