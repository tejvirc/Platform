namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    using System.Globalization;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    /// The generic currency without having major and minor symbols
    /// </summary>
    public class NoCurrency : Currency
    {
        /// <summary>
        /// The No Currency Code starts with NC
        /// </summary>
        public const string NoCurrencyCode = "NOC";

        /// <summary>
        /// The currency name for No Currency
        /// </summary>
        public const string NoCurrencyName = "No Currency";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The currency id</param>
        /// <param name="culture">The culture</param>
        public NoCurrency(int id, CultureInfo culture) :
            base(NoCurrencyCode, null, culture, null)
        {
            Id = id;
            IsoCode = NoCurrencyCode;
        }

        /// <inheritdoc/>
        public int Id { get; }

        /// <inheritdoc/>
        public override string CurrencySymbol => string.Empty;

        /// <inheritdoc/>
        public override string MinorUnitSymbol => string.Empty;

        /// <inheritdoc/>
        public override string CurrencyName => NoCurrencyName;

        /// <summary>
        /// For No Currency, always display the amount in dollar format
        /// </summary>
        public override DenomDisplayUnitType DenomDisplayUnit => DenomDisplayUnitType.Dollar;

        /// <inheritdoc/>
        public override string DisplayName => GetNoCurrencyDisplayText();

        private string GetNoCurrencyDisplayText()
        {
            string noSubUnitText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoSubUnitText);
            string decimalText = Culture.NumberFormat.CurrencyDecimalDigits > 0 ?
                $"0{Culture.NumberFormat.CurrencyDecimalSeparator}10" :
                noSubUnitText;

            const decimal defaultDescriptionAmount = 1000.00M;
            string amountText = defaultDescriptionAmount.ToString($"C{Culture.NumberFormat.CurrencyDecimalDigits}", Culture);

            string displayText = $"{NoCurrency.NoCurrencyName} {amountText} {decimalText}".Trim();
            return displayText;
        }
    }
}

