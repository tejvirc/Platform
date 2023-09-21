namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    using System.Globalization;

    /// <summary>
    /// The display unit type
    /// </summary>
    public enum DenomDisplayUnitType
    {
        /// <summary>
        /// Display denom in dollar
        /// </summary>
        Dollar = 0,
        /// <summary>
        /// Display denom in cent
        /// </summary>
        Cent = 1
    };

    /// <summary>
    /// The interface of Currency
    /// </summary>
    public interface ICurrency
    {
        /// <summary>
        /// Currency ISO code
        /// </summary>
        public string IsoCode { get; }

        /// <summary>
        /// The currency symbol
        /// </summary>
        public string CurrencySymbol { get; }

        /// <summary>
        /// Minor unit symbol
        /// </summary>
        public string MinorUnitSymbol { get; }

        /// <summary>
        /// Culture used for the currency format
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// The name of currency
        /// </summary>
        public string CurrencyName { get; }

        /// <summary>
        /// The English name of currency
        /// </summary>
        public string CurrencyEnglishName { get; }

        /// <summary>
        /// The display name of currency
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The denom display unit type, default is cent
        /// </summary>
        public DenomDisplayUnitType DenomDisplayUnit { get; }
    }
}
