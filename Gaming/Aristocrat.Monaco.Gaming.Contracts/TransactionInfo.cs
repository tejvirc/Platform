namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;

    /// <summary>
    ///     Represents transaction data associated with a game round
    /// </summary>
    [Serializable]
    public struct TransactionInfo
    {
        /// <summary>
        ///     The transaction type
        /// </summary>
        public Type TransactionType;

        /// <summary>
        ///     The amount in millicents
        /// </summary>
        public long Amount;

        /// <summary>
        ///     The time.
        /// </summary>
        public DateTime Time;

        /// <summary>
        ///     The TransactionId
        /// </summary>
        public long TransactionId;

        /// <summary>
        ///     The GameIndex from GameRoundHistory
        /// </summary>
        public int GameIndex;

        /// <summary>
        ///     The handpay type if this is a handpay transaction.
        /// </summary>
        public HandpayType? HandpayType;

        /// <summary>
        ///     Gets the key off type
        /// </summary>
        public KeyOffType? KeyOffType;

        /// <summary>
        ///     Transaction cashable amount
        /// </summary>
        public long CashableAmount;

        /// <summary>
        ///     Transaction cashable promotion amount
        /// </summary>
        public long CashablePromoAmount;

        /// <summary>
        ///     Transaction non-cashable promotion amount
        /// </summary>
        public long NonCashablePromoAmount;
    }
}
