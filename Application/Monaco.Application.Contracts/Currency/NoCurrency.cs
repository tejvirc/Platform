namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    using System.Globalization;
    using Contracts.Extensions;
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
        public NoCurrency(int id) :
            base(NoCurrencyCode, null, null)
        {
            Id = id;
            IsoCode = NoCurrencyCode;

            // Clone an culture info to set the currency format. No Currency doesn't have any specific culture associated
            Culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            ConfigureNoCurrencyCultureFormat(Id);
        }

        /// <inheritdoc/>
        public int Id { get; }

        /// <inheritdoc/>
        public override string CurrencySymbol => string.Empty;

        /// <inheritdoc/>
        public override string MinorUnitSymbol => string.Empty;

        /// <inheritdoc/>
        public override string CurrencyName => string.Empty;

        /// <inheritdoc/>
        public override string CurrencyEnglishName => NoCurrencyName;

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

            string displayText = $"{NoCurrencyName} {amountText} {decimalText}".Trim();
            return displayText;
        }

        private void ConfigureNoCurrencyCultureFormat(int id)
        {
            var format = NoCurrencyOptions.Get(id);
            Culture.ApplyNoCurrencyFormat(format);
        }
    }
}

