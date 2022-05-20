namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    /// <summary>
    ///     Event emitted when a Wat transfer has been committted.
    /// </summary>
    /// <remarks>
    ///     This event is posted when the transfer off request from the transfer host is
    ///     considered complete, whether successful or not. The client can perform any clean up code here
    ///     to prepare for normal operations.
    /// </remarks>
    public class WatTransferCommittedEvent : BaseWatEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WatTransferCommittedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The Wat transaction associated with the complete event</param>
        public WatTransferCommittedEvent(WatTransaction transaction)
            : base(transaction)
        {
        }
    }
}