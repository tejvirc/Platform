namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    using System;
    using System.Globalization;
    using Contracts.Extensions;

    /// <summary>
    /// Implement non-official currency, but configured and supported in the Monaco
    /// </summary>
    public class CustomCurrency : Currency
    {
        /// <summary>
        /// The fully customised currency, which is not avaliable from cultuer and region defined in windows
        /// </summary>
        /// <param name="currencyCode"></param>
        /// <param name="name"></param>
        /// <param name="format"></param>
        public CustomCurrency(string currencyCode, string name, ICurrencyFormatOverride format)
            : base(currencyCode, null, null)
        {
            IsoCode = !string.IsNullOrEmpty(currencyCode) ? currencyCode : throw new ArgumentNullException(nameof(currencyCode));

            CurrencyEnglishName = string.IsNullOrEmpty(name) ? throw new ArgumentNullException(nameof(name)) : name;

            Initialize(format);
        }

        /// <inheritdoc/>
        public override CultureInfo Culture { get; set; }

        /// <inheritdoc/>
        public override string CurrencyEnglishName { get; protected set; }

        /// <inheritdoc/>
        public override string Description => $"{Culture.GetFormattedDescription(this)}";

        private void Initialize(ICurrencyFormatOverride format)
        {
            Culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();

            // apply the format in the culture
            Culture.NumberFormat.CurrencySymbol = format.Symbol;
            Culture.NumberFormat.CurrencyGroupSeparator = format.GroupSeparator;
            if (!string.IsNullOrEmpty(format.DecimalSeparator))
            {
                Culture.NumberFormat.CurrencyDecimalSeparator = format.DecimalSeparator;
                Culture.NumberFormat.CurrencyDecimalDigits = format.DecimalDigits;
                MinorUnitSymbol = format.MinorUnitSymbol;
            }
            else
            {
                Culture.NumberFormat.CurrencyDecimalDigits = 0;
            }
            
        }
    }
}
