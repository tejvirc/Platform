namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    /// Event to trigger cash out dialog.
    /// </summary>
    public class CashOutDialogVisibilityEvent : BaseEvent
    {
        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        public long CashableAmount { get; }
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="amount">True if overlay window is visible</param>
        public CashOutDialogVisibilityEvent(long amount)
        {
            CashableAmount = amount;
        }
    }
}
