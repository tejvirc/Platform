namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     An event to notify that a transfer-out request has been validated and started.
    /// </summary>
    [Serializable]
    public class TransferOutStartedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferOutStartedEvent" /> class.
        /// </summary>
        /// <param name="transactionId">The unique transaction identifier</param>
        /// <param name="pendingCashableAmount">The amount to be transferred out in millicents of cashable credits.</param>
        /// <param name="pendingPromotionalAmount">The amount to be transferred out in millicents of promotional credits.</param>
        /// <param name="pendingNonCashableAmount">The amount to be transferred out in millicents of noncashable credits.</param>
        public TransferOutStartedEvent(
            Guid transactionId,
            long pendingCashableAmount,
            long pendingPromotionalAmount,
            long pendingNonCashableAmount)
        {
            TransactionId = transactionId;
            PendingCashableAmount = pendingCashableAmount;
            PendingPromotionalAmount = pendingPromotionalAmount;
            PendingNonCashableAmount = pendingNonCashableAmount;
        }

        /// <summary>
        ///      Gets the unique transaction identifier
        /// </summary>
        public Guid TransactionId { get; }

        /// <summary>
        ///     Gets the amount of cashable credits to be transferred out in millicents.
        /// </summary>
        public long PendingCashableAmount { get; }

        /// <summary>
        ///     Gets the amount of promotional credits to be transferred out in millicents.
        /// </summary>
        public long PendingPromotionalAmount { get; }

        /// <summary>
        ///     Gets the amount of non-cashable credits to be transferred out in millicents.
        /// </summary>
        public long PendingNonCashableAmount { get; }

        /// <summary>
        ///     Gets the total amount being transferred out
        /// </summary>
        public long Total => PendingCashableAmount + PendingPromotionalAmount + PendingNonCashableAmount;

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + string.Format(
                       CultureInfo.InvariantCulture,
                       " Cashable={0}, NonCashable={1}, Promo={2}",
                       PendingCashableAmount,
                       PendingNonCashableAmount,
                       PendingPromotionalAmount);
        }
    }
}