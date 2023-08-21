namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Defines a base handpay event
    /// </summary>
    [ProtoContract]
    public abstract class BaseHandpayEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        protected BaseHandpayEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseHandpayEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        protected BaseHandpayEvent(HandpayTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets the associated transaction
        /// </summary>
        [ProtoMember(1)]
        public HandpayTransaction Transaction { get; }
    }
}