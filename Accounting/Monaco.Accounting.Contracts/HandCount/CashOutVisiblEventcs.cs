namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    /// 
    /// </summary>
    public class CashOutVisiblEventcs : BaseEvent
    {
        /// <summary>
        /// True when 'Yes' is clicked in Cash out dialog
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isVisible"></param>
        public CashOutVisiblEventcs(bool isVisible)
        {
            IsVisible = isVisible;
        }
    }
}
