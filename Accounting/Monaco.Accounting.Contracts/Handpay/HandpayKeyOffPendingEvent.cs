namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using ProtoBuf;
    using System;

    /// <summary>
    ///     The event to post when waiting for a handpay key off
    /// </summary>
    [ProtoContract]
    public class HandpayKeyOffPendingEvent : BaseHandpayEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayKeyedOffEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        public HandpayKeyOffPendingEvent(HandpayTransaction transaction)
            : base(transaction)
        {
        }

        /// <summary>
        ///  Parameterless constructor used while deserializing
        /// </summary>
        public HandpayKeyOffPendingEvent()
        {
        }
    }
}