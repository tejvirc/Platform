namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using System;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Event emitted when a Hard Meter Out has completed.
    /// </summary>
    [Serializable]
    public class HardMeterOutIssuedEvent : BaseEvent
    {
        private const decimal ConvertMillicentToDollar = 100000M;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardMeterOutIssuedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        public HardMeterOutIssuedEvent(HardMeterOutTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets the associated transaction
        /// </summary>
        public HardMeterOutTransaction Transaction { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardMeterOut) + " " +
                   (Transaction.Amount / ConvertMillicentToDollar).FormattedCurrencyString();
        }

    }
}