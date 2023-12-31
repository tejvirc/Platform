﻿namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using System;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     An event to notify that a pending handpay has been keyed off.
    /// </summary>
    [Serializable]
    public class HandpayKeyedOffEvent : BaseHandpayEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayKeyedOffEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        public HandpayKeyedOffEvent(HandpayTransaction transaction)
            : base(transaction)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Localizer.For(CultureFor.Player).GetString(ResourceKeys.HandpayKeyedOff) + " " +
                   Transaction.TransactionAmount.MillicentsToDollars().FormattedCurrencyString();
        }
    }
}