namespace Aristocrat.Monaco.Application.Contracts.Extensions
{
    using System.Globalization;

    /// <summary>
    ///     CurrencyCultureData
    /// </summary>
    public class CurrencyCultureData
    {
        /// <summary>
        ///     Constructor for CurrencyCultureData
        /// </summary>
        /// <param name="valueCultureInfo">CultureInfo for the numerical value</param>
        /// <param name="wordsCultureInfo">CultureInfo for the numerical words</param>
        /// <param name="minorUnits">Minor currency units.</param>
        /// <param name="minorUnitsPlural">Minor currency units plural form.</param>
        /// <param name="majorUnitsPlural">Major currency units plural form.</param>
        /// <param name="pluralizeMajorUnits">Flag used to set the major units plural form.</param>
        /// <param name="pluralizeMinorUnits">Flag used to set the minor units plural form.</param>
        /// <param name="minorUnitSymbol">Minor Unit Symbol</param>
        /// <param name="currencyName">Minor Unit Symbol</param>
        public CurrencyCultureData(
            CultureInfo valueCultureInfo,
            CultureInfo wordsCultureInfo,
            string minorUnits,
            string minorUnitsPlural,
            string majorUnitsPlural,
            bool pluralizeMajorUnits,
            bool pluralizeMinorUnits,
            string minorUnitSymbol,
            string currencyName)
        {
            ValueCultureInfo = valueCultureInfo;
            WordsCultureInfo = wordsCultureInfo;
            MinorUnits = minorUnits;
            MinorUnitsPlural = minorUnitsPlural;
            MajorUnitsPlural = majorUnitsPlural;
            PluralizeMajorUnits = pluralizeMajorUnits;
            PluralizeMinorUnits = pluralizeMinorUnits;
            MinorUnitSymbol = minorUnitSymbol;
            CurrencyName = currencyName;
            MajorUnitsSingular = currencyName;
        }

        /// <summary>
        ///     Gets currency culture info
        /// </summary>
        public CultureInfo ValueCultureInfo { get; }

        /// <summary>
        ///     Gets currency culture info
        /// </summary>
        public CultureInfo WordsCultureInfo { get; }

        /// <summary>
        ///     Gets minor units
        /// </summary>
        public string MinorUnits { get; }

        /// <summary>
        ///     Gets minor units plural
        /// </summary>
        public string MinorUnitsPlural { get; set; }

        /// <summary>
        ///     Gets major units plural
        /// </summary>
        public string MajorUnitsPlural { get; set; }

        /// <summary>
        ///      Gets major units singular
        /// </summary>
        public string MajorUnitsSingular { get; set; }

        /// <summary>
        ///     Gets whether to pluralize major units
        /// </summary>
        public bool PluralizeMajorUnits { get; }

        /// <summary>
        ///     Gets whether to pluralize minor units
        /// </summary>
        public bool PluralizeMinorUnits { get; }

        /// <summary>
        ///     Gets the minor unit symbol
        /// </summary>
        public string MinorUnitSymbol { get; }

        /// <summary>
        ///     Name of the currency
        /// </summary>
        public string CurrencyName { get; }
    }
}