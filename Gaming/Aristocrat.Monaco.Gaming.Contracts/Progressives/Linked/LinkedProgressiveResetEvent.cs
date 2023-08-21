namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using Kernel;

    /// <summary>
    ///     This event is posted when the linked level is reset.
    /// </summary>
    public class LinkedProgressiveResetEvent : BaseEvent
    {
        /// <summary>
        /// Initializes the LinkedProgressiveResetEvent object
        /// </summary>
        /// <param name="transactionId">The id of the associated transaction</param>
        public LinkedProgressiveResetEvent(long transactionId)
        {
            TransactionId = transactionId;
        }

        /// <summary>
        ///     Gets the associated transaction id.
        /// </summary>
        public long TransactionId { get; }
    }
}
