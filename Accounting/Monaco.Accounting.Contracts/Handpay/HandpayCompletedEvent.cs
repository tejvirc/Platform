namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using ProtoBuf;
    using System;

    /// <summary>
    ///     An event to notify that a request to canceling credits has been completed.
    /// </summary>
    [ProtoContract]
    public class HandpayCompletedEvent : BaseHandpayEvent
    {

        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public HandpayCompletedEvent()
        {
        }

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