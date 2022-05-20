namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using System;

    /// <summary>
    ///     An event to notify that a request to handpay has been acknowledged.
    /// </summary>
    /// <remarks>
    ///     An request to handpay has to be acknowledged by a service implementing <c>IHandpayValidator</c>.
    /// </remarks>
    [Serializable]
    public class HandpayAcknowledgedEvent : BaseHandpayEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayAcknowledgedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        public HandpayAcknowledgedEvent(HandpayTransaction transaction)
            : base(transaction)
        {
        }
    }
}