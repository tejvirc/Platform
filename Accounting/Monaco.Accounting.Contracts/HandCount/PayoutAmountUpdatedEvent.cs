namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Kernel;

    /// <summary>
    ///     Event to trigger cash out dialog.
    /// </summary>
    public class PayoutAmountUpdatedEvent : BaseEvent
    {
        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        public long CashableAmount { get; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="amount">True if overlay window is visible</param>
        public PayoutAmountUpdatedEvent(long amount)
        {
            CashableAmount = amount;
        }
    }
}
