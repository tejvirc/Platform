namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    /// <summary>
    ///     Event emitted when the Wat host requests the cancellation of a WAT transfer
    /// </summary>
    public class WatTransferCancelRequestedEvent : BaseWatEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WatTransferCancelRequestedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The Wat transaction associated with the complete event</param>
        public WatTransferCancelRequestedEvent(WatTransaction transaction)
            : base(transaction)
        {
        }
    }
}