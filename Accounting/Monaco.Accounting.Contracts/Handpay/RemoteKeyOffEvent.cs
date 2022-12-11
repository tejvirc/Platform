namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     An event for the conversation between the backend and the components which
    ///     require the key-off from the operator.
    /// </summary>
    [ProtoContract]
    public class RemoteKeyOffEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public RemoteKeyOffEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RemoteKeyOffEvent" /> class.
        /// </summary>
        /// <param name="keyOffType">The key off type</param>
        /// <param name="cashableAmount">The cashable amount requiring a handpay</param>
        /// <param name="promoAmount">The promotional amount requiring a handpay</param>
        /// <param name="nonCashAmount">The non-cashable amount requiring a handpay</param>
        ///<param name="selectedByHost">Indicate that KeyOffType is initiated by host</param>
        public RemoteKeyOffEvent(KeyOffType keyOffType, long cashableAmount, long promoAmount, long nonCashAmount, bool selectedByHost = true)
        {
            KeyOffType = keyOffType;
            CashableAmount = cashableAmount;
            PromoAmount = promoAmount;
            NonCashAmount = nonCashAmount;
            SelectedByHost = selectedByHost;
        }

        /// <summary>
        ///     Gets the key off type
        /// </summary>
        [ProtoMember(1)]
        public KeyOffType KeyOffType { get; }

        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        [ProtoMember(2)]
        public long CashableAmount { get; }

        /// <summary>
        ///     Gets the promo amount
        /// </summary>
        [ProtoMember(3)]
        public long PromoAmount { get; }

        /// <summary>
        ///     Gets the non-cashable amount
        /// </summary>
        [ProtoMember(4)]
        public long NonCashAmount { get; }

        /// <summary>
        ///     Indicate that KeyOffType is initiated by host
        /// </summary>
        [ProtoMember(5)]
        public bool SelectedByHost { get; }
    }
}