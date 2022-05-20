namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     An event to notify that a transfer-out request has been completed successfully.
    /// </summary>
    [Serializable]
    public class TransferOutCompletedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferOutCompletedEvent" /> class.
        /// </summary>
        /// <param name="cashableAmount">The amount transferred out in millicents of cashable credits.</param>
        /// <param name="promotionalAmount">The amount transferred out in millicents of promotional credits.</param>
        /// <param name="nonCashableAmount">The amount transferred out in millicents of non-cashable credits.</param>
        /// <param name="pending">true if there is a pending transfer</param>
        /// <param name="traceId">A unique Id that can be used to track the transfer</param>
        public TransferOutCompletedEvent(
            long cashableAmount,
            long promotionalAmount,
            long nonCashableAmount,
            bool pending,
            Guid traceId)
        {
            CashableAmount = cashableAmount;
            PromotionalAmount = promotionalAmount;
            NonCashableAmount = nonCashableAmount;
            Pending = pending;
            TraceId = traceId;
        }

        /// <summary>
        ///     Gets the amount of cashable credits transferred out in millicents.
        /// </summary>
        public long CashableAmount { get; }

        /// <summary>
        ///     Gets the amount of promotional credits transferred out in millicents.
        /// </summary>
        public long PromotionalAmount { get; }

        /// <summary>
        ///     Gets the amount of non-cashable credits transferred out in millicents.
        /// </summary>
        public long NonCashableAmount { get; }

        /// <summary>
        ///     Gets a value indicating whether or not a transfer is pending
        /// </summary>
        public bool Pending { get; }

        /// <summary>
        ///     Gets a unique Id that can be used to track the transfer
        /// </summary>
        public Guid TraceId { get; }

        /// <summary>
        ///     Gets the total amount transferred out in millicents.
        /// </summary>
        public long Total => CashableAmount + PromotionalAmount + NonCashableAmount;

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + string.Format(
                       CultureInfo.InvariantCulture,
                       " Cashable={0}, NonCashable={1}, Promo={2}, TraceId={3} Pending={4}",
                       CashableAmount,
                       NonCashableAmount,
                       PromotionalAmount,
                       TraceId,
                       Pending);
        }
    }
}