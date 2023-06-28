namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Kernel;

    /// <summary>
    ///     Event to decide whether to proceed cash out or not.
    /// </summary>
    public class CashoutAmountAuthorizationReceivedEvent : BaseEvent
    {
        /// <summary>
        ///     true when we can cash out
        /// </summary>
        public bool IsCashout { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CashoutAmountAuthorizationReceivedEvent" /> class.
        /// </summary>
        /// <param name="isCashOut"></param>
        public CashoutAmountAuthorizationReceivedEvent(bool isCashOut)
        {
            IsCashout = isCashOut;
        }
    }
}
