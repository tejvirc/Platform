namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    /// <summary>
    ///     Event emitted when a Wat transfer has been requested by the Wat host.
    /// </summary>
    /// <remarks>
    ///     This event is posted when the transfer off request is received from the transfer host
    ///     (or the IWatTransferOffProvider). This signals the start of the transfer. This can be used by the
    ///     client to perform any messaging to the user, but otherwise the client should not allow any other
    ///     actions to take place.
    /// </remarks>
    public class WatTransferRequestedEvent : BaseWatEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WatTransferRequestedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The Wat transaction associated with the event</param>
        public WatTransferRequestedEvent(WatTransaction transaction)
            : base(transaction)
        {
        }
    }
}