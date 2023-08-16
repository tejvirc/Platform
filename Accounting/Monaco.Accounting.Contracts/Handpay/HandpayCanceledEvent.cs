namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using System;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Localization.Properties;
    using ProtoBuf;

    /// <summary>
    ///     An event to notify that a request to handpay has been cancelled.
    /// </summary>
    [ProtoContract]
    public class HandpayCanceledEvent : BaseHandpayEvent
    {

        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public HandpayCanceledEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayCanceledEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        public HandpayCanceledEvent(HandpayTransaction transaction)
            : base(transaction)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var total = Transaction.CashableAmount + Transaction.PromoAmount + Transaction.NonCashAmount;
            return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HandpayTransactionCanceled) + ": " +
                   total.MillicentsToDollars().FormattedCurrencyString();
        }
    }
}