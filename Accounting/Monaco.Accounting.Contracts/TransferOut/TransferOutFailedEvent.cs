namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Definition of the TransferOutFailedEvent class
    /// </summary>
    public class TransferOutFailedEvent : BaseEvent
    {
        private const decimal ConvertMillicentToDollar = 100000M;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferOutFailedEvent" /> class.
        /// </summary>
        /// <param name="cashableAmount">The failed amount transferred out in millicents of cashable credits.</param>
        /// <param name="promotionalAmount">The failed amount transferred out in millicents of promotional credits.</param>
        /// <param name="nonCashableAmount">The failed amount transferred out in millicents of non-cashable credits.</param>
        /// <param name="traceId">A unique Id that can be used to track the transfer</param>
        public TransferOutFailedEvent(long cashableAmount, long promotionalAmount, long nonCashableAmount, Guid traceId)
        {
            CashableAmount = cashableAmount;
            PromotionalAmount = promotionalAmount;
            NonCashableAmount = nonCashableAmount;
            TraceId = traceId;
        }

        /// <summary>
        ///     Gets the amount of cashable credits that failed transferred out in millicents.
        /// </summary>
        public long CashableAmount { get; }

        /// <summary>
        ///     Gets the amount of promotional credits that failed transferred out in millicents.
        /// </summary>
        public long PromotionalAmount { get; }

        /// <summary>
        ///     Gets the amount of non-cashable credits that failed transferred out in millicents.
        /// </summary>
        public long NonCashableAmount { get; }

        /// <summary>
        ///     Gets a unique Id that can be used to track the transfer
        /// </summary>
        public Guid TraceId { get; }

        /// <summary>
        ///     Gets the total amount that failed transferred out in millicents.
        /// </summary>
        public long Total => CashableAmount + PromotionalAmount + NonCashableAmount;

        /// <inheritdoc />
        public override string ToString()
        {
            return Localizer.For(CultureFor.Player).GetString(ResourceKeys.TransferOutFailedText) + " " +
                   (Total / ConvertMillicentToDollar).FormattedCurrencyString();
        }
    }
}