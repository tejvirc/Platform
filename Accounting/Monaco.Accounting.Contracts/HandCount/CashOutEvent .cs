namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    /// Event to decide whether to proceed cash out or not.
    /// </summary>
    public class CashOutEvent : BaseEvent
    {
        /// <summary>
        /// true when we can cash out
        /// </summary>
        public bool IsCashout { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isCashOut"></param>
        public CashOutEvent(bool isCashOut)
        {
            IsCashout = isCashOut;
        }
    }
}
