namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Kernel.MarketConfig.Models.Accounting;
    using ProtoBuf;

    /// <summary>
    ///     Represents transaction data associated with a game round
    /// </summary>
    [ProtoContract]
    public struct TransactionInfo
    {
        /// <summary>
        ///     The transaction type
        /// </summary>
        [ProtoMember(1)]
        public Type TransactionType;

        /// <summary>
        ///     The amount
        /// </summary>
        [ProtoMember(2)]
        public long Amount;

        /// <summary>
        ///     The time.
        /// </summary>
        [ProtoMember(3)]
        public DateTime Time;

        /// <summary>
        ///     The TransactionId
        /// </summary>
        [ProtoMember(4)]
        public long TransactionId;

        /// <summary>
        ///     The GameIndex from GameRoundHistory
        /// </summary>
        [ProtoMember(5)]
        public int GameIndex;

        /// <summary>
        ///     The handpay type if this is a handpay transaction.
        /// </summary>
        [ProtoMember(6)]
        public HandpayType? HandpayType;

        /// <summary>
        ///     Gets the key off type
        /// </summary>
        [ProtoMember(7)]
        public KeyOffType? KeyOffType;

        /// <summary>
        ///     Transaction cashable amount
        /// </summary>
        [ProtoMember(8)]
        public long CashableAmount;

        /// <summary>
        ///     Transaction cashable promotion amount
        /// </summary>
        [ProtoMember(9)]
        public long CashablePromoAmount;

        /// <summary>
        ///     Transaction non-cashable promotion amount
        /// </summary>
        [ProtoMember(10)]
        public long NonCashablePromoAmount;
    }
}
