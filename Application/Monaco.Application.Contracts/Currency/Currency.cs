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
        /// Constructor
        /// </summary>
        /// <param name="isoCurrencyCode">Currency code</param>
        /// <param name="region">The region</param>
        /// <param name="culture">The culture</param>
        /// <param name="minorUnitSymbol">minor unit symbol</param>
        public Currency(string isoCurrencyCode, RegionInfo region, CultureInfo culture, string minorUnitSymbol = null)
        {
            IsoCode = isoCurrencyCode;
            Description = culture.GetFormattedDescription(region);
            DescriptionWithMinorSymbol = CurrencyExtensions.GetDescriptionWithMinorSymbol(Description, minorUnitSymbol);
        }

        /// <summary>
        /// Currency ISO code
        /// </summary>
        public string IsoCode { get; }

        /// <summary>
        /// Description of currency
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Description with minor symbol
        /// </summary>
        public string DescriptionWithMinorSymbol { get; }
    }
}