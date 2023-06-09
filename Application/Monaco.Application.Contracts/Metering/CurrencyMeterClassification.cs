﻿namespace Aristocrat.Monaco.Application.Contracts
{
    using Extensions;
    using Kernel;
    using System.Globalization;

    /// <summary>
    ///     A classification of meters that represent an amount of money,
    ///     and rolls over at 99,999,999.99999 (8 significant plus 2 insignificant digits)
    /// </summary>
    public class CurrencyMeterClassification : MeterClassification
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyMeterClassification" /> class.
        /// </summary>
        public CurrencyMeterClassification()
            : base("Currency", ServiceManager.GetInstance().GetService<IPropertiesManager>().GetValue(ApplicationConstants.CurrencyMeterRolloverText, 10000000000000L))
        {
        }

        /// <summary>
        ///     Creates and returns a string representation of the value
        /// </summary>
        /// <param name="meterValue">The value in to convert to a string</param>
        /// <param name="culture">The optional CultureInfo to use for string formatting</param>
        /// <returns>A string representation of the value</returns>
        public override string CreateValueString(long meterValue, CultureInfo culture = null)
        {
            var currencyMultiplier = ServiceManager.GetInstance().GetService<IPropertiesManager>().GetValue(ApplicationConstants.CurrencyMultiplierKey, 0.0);
            var inCents = meterValue / currencyMultiplier;

            return inCents.FormattedCurrencyString(culture: culture);
        }
    }
}