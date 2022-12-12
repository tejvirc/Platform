namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using System;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     An event to notify that a request to cancel credits has been started to process.
    /// </summary>
    [Serializable]
    public class HandpayStartedEvent : BaseEvent
    {
        private const decimal ConvertMillicentToDollar = 100000M;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayStartedEvent" /> class.
        /// </summary>
        /// <param name="handpayType">The handpay type</param>
        /// <param name="cashableAmount">The cashable amount requiring a handpay</param>
        /// <param name="promoAmount">The promotional amount requiring a handpay</param>
        /// <param name="nonCashAmount">The non-cashable amount requiring a handpay</param>
        /// <param name="wagerAmount">wager responsible for large win handpay</param>
        /// <param name="eligibleResetToCreditMeter">would be true if current handpay transaction can be reset to credit meter</param>
        public HandpayStartedEvent(
            HandpayType handpayType,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            long wagerAmount,
            bool eligibleResetToCreditMeter)
        {
            Handpay = handpayType;
            CashableAmount = cashableAmount;
            PromoAmount = promoAmount;
            NonCashAmount = nonCashAmount;
            WagerAmount = wagerAmount;
            EligibleResetToCreditMeter = eligibleResetToCreditMeter;
        }

        /// <summary>
        ///     Gets the type of handpay
        /// </summary>
        public HandpayType Handpay { get; }

        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        public long CashableAmount { get; }

        /// <summary>
        ///     Gets the promo amount
        /// </summary>
        public long PromoAmount { get; }

        /// <summary>
        ///     Gets the non-cashable amount
        /// </summary>
        public long NonCashAmount { get; }

        /// <summary>
        ///     Gets the wager amount
        /// </summary>
        public long WagerAmount { get; }

        /// <summary>true if current handpay transaction can be reset to credit meter.</summary>
        public bool EligibleResetToCreditMeter { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var total = CashableAmount + PromoAmount + NonCashAmount;
            return Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.HandpayStartedText) + " " +
                   (total / ConvertMillicentToDollar).FormattedCurrencyString();
        }
    }
}