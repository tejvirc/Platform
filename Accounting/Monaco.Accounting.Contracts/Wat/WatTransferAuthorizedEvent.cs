namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    /// <summary>
    ///     Event emitted when a Wat transfer has been authorized
    /// </summary>
    public class WatTransferAuthorizedEvent : BaseWatEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WatTransferAuthorizedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The Wat transaction associated with the event</param>
        public WatTransferAuthorizedEvent(WatTransaction transaction)
            : base(transaction)
        {
        }
    }
}