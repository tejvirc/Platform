namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event emitted when a VoucherOut has started.
    /// </summary>
    [ProtoContract]
    public class VoucherOutStartedEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public VoucherOutStartedEvent()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="amount"></param>
        public VoucherOutStartedEvent(long amount)
        {
            Amount = amount;
        }
        /// <summary>
        /// Amount
        /// </summary>
        [ProtoMember(1)]
        public long Amount { get; }
    }
}