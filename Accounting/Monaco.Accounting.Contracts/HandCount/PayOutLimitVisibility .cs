namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    /// Event to show the payout lock up
    /// </summary>
    public class PayOutLimitVisibility : BaseEvent
    {
        /// <summary>
        /// payout lock up is shown when true
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPrintingVisible { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isVisible"></param>
        /// <param name="isPrintingVisible"></param>
        public PayOutLimitVisibility(bool isVisible, bool isPrintingVisible)
        {
            IsVisible = isVisible;
            IsPrintingVisible = isPrintingVisible;
        }
    }
}
