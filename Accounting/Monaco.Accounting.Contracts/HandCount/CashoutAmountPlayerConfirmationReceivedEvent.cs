namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    ///     Event to decide whether to proceed cash out or not.
    /// </summary>
    public class CashoutAmountPlayerConfirmationReceivedEvent : BaseEvent
    {
        /// <summary>
        /// true when we can cash out
        /// </summary>
        public bool IsCashout { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CashoutAmountPlayerConfirmationReceivedEvent" /> class.
        /// </summary>
        /// <param name="isCashOut"></param>
        public CashoutAmountPlayerConfirmationReceivedEvent(bool isCashOut)
        {
            IsCashout = isCashOut;
        }
    }
}
