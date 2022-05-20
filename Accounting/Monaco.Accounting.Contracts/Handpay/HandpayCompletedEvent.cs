namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using System;

    /// <summary>
    ///     An event to notify that a request to canceling credits has been completed.
    /// </summary>
    [Serializable]
    public class HandpayCompletedEvent : BaseHandpayEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayCompletedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        public HandpayCompletedEvent(HandpayTransaction transaction)
            : base(transaction)
        {
        }
    }
}