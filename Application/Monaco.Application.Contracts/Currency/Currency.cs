namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    using System.Globalization;
    using Extensions;

    /// <summary>
    /// Currency with its format
    /// </summary>
    public class Currency
    {
        /// <summary>
        /// The default value of currency multiplier
        /// </summary>
        public static readonly long DefaultCurrencyMultiplier = 1M.DollarsToMillicents();

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
        /// Constructor
        /// </summary>
        /// <param name="isoCurrencyCode">Currency code</param>
        /// <param name="region">The region</param>
        /// <param name="culture">The culture</param>
        /// <param name="minorUnitSymbol">minor unit symbol</param>
        public Currency(string isoCurrencyCode, RegionInfo region, CultureInfo culture, string minorUnitSymbol = null)
        {
            IsoCode = isoCurrencyCode;
            MinorUnitSymbol = minorUnitSymbol;
            Description = culture.GetFormattedCurrencyDescription(isoCurrencyCode, region);

            Culture = culture;
        }

        /// <summary>
        /// Currency ISO code
        /// </summary>
        public string IsoCode { get; protected set; }

        /// <summary>
        /// Description of currency
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// The currency symbol
        /// </summary>
        public virtual string CurrencySymbol => Culture.NumberFormat.CurrencySymbol;

        /// <summary>
        /// Minor unit symbol
        /// </summary>
        public virtual string MinorUnitSymbol { get; }

        /// <summary>
        /// Culture used for the currency format
        /// </summary>
        public CultureInfo Culture { get; protected set; }

        /// <summary>
        /// The currency's english name
        /// </summary>
        public virtual string CurrencyName
        {
            get
            {
                RegionInfo region = new RegionInfo(Culture.Name);
                var currencyNameArray = region.CurrencyEnglishName.Split(' ');
                if (currencyNameArray.Length > 0)
                {
                    return currencyNameArray[currencyNameArray.Length - 1];
                }

                return IsoCode;
            }
        }

        /// <summary>
        /// The denom display unit type, default is cent
        /// </summary>
        public virtual DenomDisplayUnitType DenomDisplayUnit => DenomDisplayUnitType.Cent;

        /// <summary>
        ///     Gets Description with minor currency symbol of the current currency.
        /// </summary>
        public virtual string DisplayName =>
            string.IsNullOrEmpty(MinorUnitSymbol)
                ? $"{Description}"
                : $"{Description} 10{MinorUnitSymbol}";
    }
}