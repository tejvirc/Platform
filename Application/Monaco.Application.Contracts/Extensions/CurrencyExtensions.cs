namespace Aristocrat.Monaco.Application.Contracts.Extensions
{
    using System;
    using System.Globalization;
    using Common;
    using Currency;
    using Kernel;
    using Localization;
    using log4net;
    using Monaco.Localization.Properties;
    using OperatorMenu;

    /// <summary>
    ///     CurrencyExtensions
    /// </summary>
    public static class CurrencyExtensions
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CurrencyExtensions));

        private const string And = " and ";
        private const string Space = " ";
        private const decimal OneTrillion = 1.00m * 1000m * 1000m * 1000m * 1000m;
        private const decimal ZeroDollars = 0.0M;
        private const int MillicentPerCentsDigits = 3;
        private const long MillicentsPerCent = 1000;
        private static string _minorUnits = Resources.Cent;
        private static string _majorUnitsPlural;
        private static string _minorUnitsPlural;

        private static IOperatorMenuConfiguration _configuration;

        /// <summary>
        /// The default amount used to generate currency description
        /// </summary>
        public const decimal DefaultDescriptionAmount = 1000.00M;

        private static IOperatorMenuConfiguration Configuration =>
            _configuration ??= ServiceManager.GetInstance().TryGetService<IOperatorMenuConfiguration>();

        private static bool UseOperatorCultureForCurrencyFormatting => Configuration?.GetSetting(OperatorMenuSetting.UseOperatorCultureForCurrencyFormatting, false) ?? false;

        private static CultureInfo CurrencyDisplayCulture => UseOperatorCultureForCurrencyFormatting ? Localizer.For(CultureFor.Operator).CurrentCulture : CurrencyExtensions.CurrencyCultureInfo;

        /// <summary>
        /// The configured currency
        /// </summary>
        public static Currency Currency { get; set; }

        /// <summary>
        ///     Gets the currency minor Units per major.
        /// </summary>
        public static decimal CurrencyMinorUnitsPerMajorUnit { get; private set; } = 100M;

        /// <summary>
        ///     Gets currency minor symbol.
        /// </summary>
        public static string MinorUnitSymbol { get; private set; } = string.Empty;

        /// <summary>
        ///     Gets the currency culture info.
        /// </summary>
        public static CultureInfo CurrencyCultureInfo { get; private set; } = CultureInfo.CurrentCulture;

        private static decimal MillicentDivisor => MillicentsPerCent * CurrencyMinorUnitsPerMajorUnit;

        /// <summary>
        ///     Check if the dollar value is below the given minimum.
        /// </summary>
        /// <param name="dollars">The value in dollars.</param>
        /// <param name="minimum">The minimum value.</param>
        /// <returns>True if the dollar value is below the given minimum, false if not.</returns>
        public static bool IsBelowMinimum(this decimal dollars, long minimum = 0)
        {
            return dollars * MillicentDivisor < minimum;
        }

        /// <summary>
        ///     Check if the dollar value is below the maximum.
        /// </summary>
        /// <param name="dollars">The value in dollars.</param>
        /// <param name="maximum">The maximum value in dollars.</param>
        /// <returns>True if the dollar value is below the maximum, false if not.</returns>
        public static bool IsBelowMaximum(this decimal dollars, decimal maximum)
        {
            return maximum * MillicentDivisor >= dollars * MillicentDivisor;
        }

        /// <summary>
        ///     Converts millicents to dollars.
        /// </summary>
        /// <param name="millicents">The millicents to convert into dollars.</param>
        /// <returns>Dollar amount for the provided millicents.</returns>
        public static decimal MillicentsToDollars(this long millicents)
        {
            var dollars = millicents / MillicentDivisor;
            if (millicents == long.MaxValue)
            {
                dollars = Math.Floor(millicents / MillicentDivisor * CurrencyMinorUnitsPerMajorUnit) /
                          CurrencyMinorUnitsPerMajorUnit;
            }

            return dollars;
        }

        /// <summary>
        ///     Converts millicents to dollars with fractional units. The normal
        ///     millicents to dollars will allow a fractional unit which can lead
        ///     to rounding if being represented as a currency.
        /// </summary>
        /// <param name="millicents">The millicents to convert into dollars.</param>
        /// <returns>Dollar amount for the provided millicents.</returns>
        public static decimal MillicentsToDollarsNoFraction(this long millicents)
        {
            var fractionalAmount = millicents % MillicentsPerCent;
            millicents -= fractionalAmount;

            var dollars = millicents / MillicentDivisor;
            if (millicents == long.MaxValue)
            {
                dollars = Math.Floor(millicents / MillicentDivisor * CurrencyMinorUnitsPerMajorUnit) /
                          CurrencyMinorUnitsPerMajorUnit;
            }

            return dollars;
        }

        /// <summary>
        ///     Remove the fractional units from the millicents.
        /// </summary>
        /// <param name="millicents">The millicents to remove fraction.</param>
        /// <returns>Millicents without fraction units for the provided millicents.</returns>
        public static long RemoveMillicentsFraction(this long millicents) => millicents.MillicentsToDollarsNoFraction().DollarsToMillicents();

        /// <summary>
        ///     Converts dollars to millicents.
        /// </summary>
        /// <param name="dollars">The value in dollars.</param>
        /// <returns>Millicents amount for the provided dollars.</returns>
        public static long DollarsToMillicents(this decimal dollars)
        {
            return dollars * MillicentDivisor > long.MaxValue ? long.MaxValue : (long)(dollars * MillicentDivisor);
        }

        /// <summary>
        ///     Converts dollars to millicents.
        /// </summary>
        /// <param name="dollars">The value in dollars.</param>
        /// <returns>Millicents amount for the provided dollars.</returns>
        public static long DollarsToMillicents(this long dollars)
        {
            return ((decimal)dollars).DollarsToMillicents();
        }

        /// <summary>
        ///     Converts dollars to cents.
        /// </summary>
        /// <param name="dollars">The value in dollars.</param>
        /// <returns>Dollar value for the provided cents.</returns>
        public static long DollarsToCents(this decimal dollars)
        {
            return dollars * CurrencyMinorUnitsPerMajorUnit > long.MaxValue
                ? long.MaxValue
                : (long)(dollars * CurrencyMinorUnitsPerMajorUnit);
        }

        /// <summary>
        ///     Converts millicents to cents.
        /// </summary>
        /// <param name="millicents">The millicents to convert into cents.</param>
        /// <returns>Cents amount for the provided millicents.</returns>
        public static long MillicentsToCents(this long millicents)
        {
            var dollars = millicents.MillicentsToDollars();
            return dollars.DollarsToMillicents() / MillicentsPerCent;
        }

        /// <summary>
        ///     Converts cents to millicents.
        /// </summary>
        /// <param name="cents">The cents to convert into millicents.</param>
        /// <returns>Millicents amount for the provided cents.</returns>
        public static long CentsToMillicents(this long cents)
        {
            return cents * MillicentsPerCent;
        }

        /// <summary>
        ///     Converts cents to dollars.
        /// </summary>
        /// <param name="cents">The cents to convert into dollars.</param>
        /// <returns>Dollars amount for the provided cents.</returns>
        public static decimal CentsToDollars(this long cents)
        {
            return cents / CurrencyMinorUnitsPerMajorUnit;
        }

        /// <summary>
        ///     Validate a dollar amount
        /// </summary>
        /// <param name="dollars">The dollars being validated</param>
        /// <param name="canEqualZero">Flag whether dollars is valid when 0.</param>
        /// <param name="maximum">A maximum value to check the dollar value against.</param>
        /// <param name="minimum">A minimum value to check the dollar value against.</param>
        /// <returns>null if valid and error string if invalid</returns>
        public static string Validate(
            this decimal dollars,
            bool canEqualZero = false,
            long maximum = 0,
            long minimum = 0)
        {
            var maximumInDollars = maximum == 0 ? long.MaxValue.MillicentsToDollars() : maximum.MillicentsToDollars();
            if (!dollars.IsBelowMaximum(maximumInDollars))
            {
                return string.Format(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LessThanOrEqualErrorMessage),
                    maximumInDollars.FormattedCurrencyString());
            }

            if (dollars.IsBelowMinimum(minimum) && minimum > 0)
            {
                return string.Format(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GreaterThanOrEqualErrorMessage),
                    minimum.MillicentsToDollars().FormattedCurrencyString());
            }

            if (!(dollars > ZeroDollars || canEqualZero && dollars == ZeroDollars))
            {
                return string.Format(
                    canEqualZero
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GreaterThanOrEqualErrorMessage)
                        : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GreaterThanErrorMessage),
                    ZeroDollars.FormattedCurrencyString());
            }

            return null;
        }

        /// <summary>
        ///     Validate a decimal amount
        /// </summary>
        /// <param name="amount">The amount being validated</param>
        /// <param name="maximum">A maximum value to check the amount against.</param>
        /// <param name="minimum">A minimum value to check the amount against.</param>
        /// <returns>null if valid and error string if invalid</returns>
        public static string ValidateDecimal(
            this decimal amount,
            decimal minimum = decimal.MinValue,
            decimal maximum = decimal.MaxValue)
        {

            if (amount > maximum)
            {
                return string.Format(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LessThanOrEqualErrorMessage),
                    maximum);
            }

            if (amount < minimum)
            {
                return string.Format(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GreaterThanOrEqualErrorMessage),
                    minimum);
            }

            return null;
        }

        /// <summary>
        ///     Formats currency string using Operator defined culture.
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="withMillicents">Whether to add extra digits for fractional currency (default false)</param>
        /// <returns></returns>
        public static string FormattedCurrencyStringForOperator(this decimal amount, bool withMillicents = false)
        {
            return FormattedCurrencyString(amount, withMillicents, CurrencyDisplayCulture);
        }

        /// <summary>
        ///     Formats currency string using Operator defined culture.
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="withMillicents">Whether to add extra digits for fractional currency (default false)</param>
        /// <returns></returns>
        public static string FormattedCurrencyStringForOperator(this double amount, bool withMillicents = false)
        {
            return FormattedCurrencyString(amount, withMillicents, CurrencyDisplayCulture);
        }

        /// <summary>
        ///     Formats currency string.
        /// </summary>
        /// <param name="amount">Amount in dollars.</param>
        /// <param name="withMillicents">Whether to add extra digits for fractional currency (default false)</param>
        /// <param name="culture">The optional culture used for formatting.  If none is provided, defaults to the current Currency culture.</param>
        /// <returns>Formatted currency string.</returns>
        public static string FormattedCurrencyString(this decimal amount, bool withMillicents = false, CultureInfo culture = null)
        {
            culture ??= CurrencyCultureInfo;
            var subUnitDigits = culture.NumberFormat.CurrencyDecimalDigits;
            if (withMillicents)
            {
                subUnitDigits += MillicentPerCentsDigits;
            }

            return amount.ToString($"C{subUnitDigits}", culture);
        }

        /// <summary>
        ///     Formats currency string.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <param name="format">Format override.</param>
        /// <param name="culture">The optional CultureInfo to use for string formatting</param>
        /// <returns>Formatted currency string.</returns>
        public static string FormattedCurrencyString(this int amount, string format = null, CultureInfo culture = null)
        {
            culture ??= CurrencyCultureInfo;
            return amount.ToString(format ?? "C", culture);
        }

        /// <summary>
        ///     Formats currency string.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <param name="withMillicents">Whether to add extra digits for fractional currency (default false)</param>\
        /// <param name="culture">The optional CultureInfo to use for string formatting</param>
        /// <returns>Formatted currency string.</returns>
        public static string FormattedCurrencyString(this string amount, bool withMillicents = false, CultureInfo culture = null)
        {
            culture ??= CurrencyCultureInfo;
            return double.TryParse(amount, out var result) ? result.FormattedCurrencyString(withMillicents, culture) : amount;
        }

        /// <summary>
        ///     Formats currency string.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <param name="withMillicents">Whether to add extra digits for fractional currency (default false)</param>
        /// <param name="culture">The optional CultureInfo to use for string formatting</param>
        /// <returns>Formatted currency string.</returns>
        public static string FormattedCurrencyString(this double amount, bool withMillicents = false, CultureInfo culture = null)
        {
            culture ??= CurrencyCultureInfo;
            var subUnitDigits = culture.NumberFormat.CurrencyDecimalDigits;
            if (withMillicents)
            {
                subUnitDigits += MillicentPerCentsDigits;
            }

            return amount.ToString($"C{subUnitDigits}", culture);
        }

        /// <summary>
        ///     Formats currency string.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <param name="withMillicents">Whether to add extra digits for fractional currency (default false)</param>
        /// <param name="culture">The optional culture used for formatting.  If none is provided, defaults to the current Currency culture.</param>
        /// <returns>Formatted currency string.</returns>
        public static string FormattedCurrencyString(this long amount, bool withMillicents = false, CultureInfo culture = null)
        {
            culture ??= CurrencyCultureInfo;

            var subUnitDigits = culture.NumberFormat.CurrencyDecimalDigits;
            if (withMillicents)
            {
                subUnitDigits += MillicentPerCentsDigits;
            }

            return amount.ToString($"C{subUnitDigits}", culture);
        }

        /// <summary>
        ///     Formats denom string.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <returns>Formatted denom string.</returns>
        public static string FormattedDenomString(this long amount)
        {
            // check if the currency has symbols
            bool isNoCurrency = string.IsNullOrEmpty(CurrencyCultureInfo.NumberFormat.CurrencySymbol) &&
                                string.IsNullOrEmpty(Currency.MinorUnitSymbol);
            if (isNoCurrency)
            {
                // for No Currency with subunits, we will show two digits in subunits
                // eg. No Currency 1,000.00 10c, we display denoms as 0.01, 0.02, 0.05, 0.10, 1, 2, 5, etc
                //     No Currency 1,000, we display denoms as 1, 2, 5, 10, 100, 200, 500, etc
                return amount.CentsToDollars().ToString(
                    $"C{CurrencyCultureInfo.NumberFormat.CurrencyDecimalDigits}",
                    CurrencyCultureInfo);
            }

            return amount >= CurrencyMinorUnitsPerMajorUnit
                ? amount.CentsToDollars().ToString("C0", CurrencyCultureInfo)
                : $"{amount}{Currency.MinorUnitSymbol}";
        }

        /// <summary>
        ///     Set culture information for the currency string.
        /// </summary>
        /// <param name="currencyCode">currency code</param>
        /// <param name="cultureInfo">CultureInfo.</param>
        /// <param name="minorUnits">Minor currency units.</param>
        /// <param name="minorUnitsPlural">Minor currency units plural form.</param>
        /// <param name="pluralizeMajorUnits">Flag used to set the major units plural form.</param>
        /// <param name="pluralizeMinorUnits">Flag used to set the minor units plural form.</param>
        /// <param name="minorUnitSymbol">Minor Unit Symbol</param>
        /// <returns>Dollars to millicents conversion</returns>
        public static long SetCultureInfo(
            string currencyCode,
            CultureInfo cultureInfo,
            string minorUnits = null,
            string minorUnitsPlural = null,
            bool pluralizeMajorUnits = false,
            bool pluralizeMinorUnits = false,
            string minorUnitSymbol = null)
        {
            CurrencyCultureInfo = cultureInfo;
            CurrencyMinorUnitsPerMajorUnit = Convert.ToDecimal(
                Math.Pow(
                    cultureInfo.NumberFormat.NativeDigits.Length,
                    cultureInfo.NumberFormat.CurrencyDecimalDigits));
            MinorUnitSymbol = minorUnitSymbol ?? string.Empty;

            RegionInfo region = null;

            if (!string.IsNullOrEmpty(CurrencyCultureInfo.Name))
            {
                region = new RegionInfo(CurrencyCultureInfo.Name);
            }

            if (region != null)
            {
                if (Currency == null)
                {
                    throw new ArgumentException("Currency is not configured");
                }
                _majorUnitsPlural = pluralizeMajorUnits
                    ? Currency.CurrencyName.PluralizeWord()
                    : Currency.CurrencyName;

                if (!string.IsNullOrEmpty(minorUnits))
                {
                    _minorUnits = minorUnits;
                }

                if (!string.IsNullOrEmpty(minorUnitsPlural))
                {
                    _minorUnitsPlural = minorUnitsPlural;
                }
                else
                {
                    _minorUnitsPlural = pluralizeMinorUnits
                        ? _minorUnits.PluralizeWord()
                        : _minorUnits;
                }
            }

            return 1M.DollarsToMillicents();
        }

        /// <summary>This function will convert a currency to its word representation.</summary>
        /// <param name="amount">The amount.</param>
        /// <param name="toUpper">Whether or not to convert the words to upper case.</param>
        /// <returns>The amount in words.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when argument is one trillion or larger</exception>
        public static string ConvertCurrencyToWords(decimal amount, bool toUpper = true)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, @"Argument must be positive");
            }

            if (amount >= OneTrillion)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, @"Amount must be smaller than one trillion");
            }

            var majorUnits = (long)amount;
            var minorUnits = (int)((amount - majorUnits) * 100);
            var textualAmount = majorUnits == 1
                ? $"{majorUnits.NumberToWords(CurrencyCultureInfo)} {Currency.CurrencyName}"
                : $"{majorUnits.NumberToWords(CurrencyCultureInfo)} {_majorUnitsPlural}";

            if (CurrencyCultureInfo.NumberFormat.CurrencyDecimalDigits == 0)
            {
                return toUpper ? textualAmount.ToUpper(CurrencyCultureInfo) : textualAmount;
            }

            var minorUnitsText = minorUnits == 1 ? _minorUnits : _minorUnitsPlural;
            textualAmount =
                $"{textualAmount.Replace(And, Space)} {Localizer.For(CultureFor.Player).GetString(CurrencyCultureInfo, ResourceKeys.And)} {minorUnits.NumberToWords(CurrencyCultureInfo)} {minorUnitsText}";
            return toUpper ? textualAmount.ToUpper(CurrencyCultureInfo) : textualAmount;
        }

        /// <summary>
        /// Gets the formatted currency description for the specified region.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <param name="isoCurrencyCode">currency code</param>
        /// <param name="region">The region.</param>
        /// <returns>The formatted description, which includes the currency's English name, ISO symbol, and formatted currency value.</returns>
        public static string GetFormattedDescription(this CultureInfo culture, string isoCurrencyCode, RegionInfo region = null)
        {
            region ??= !string.IsNullOrEmpty(culture.Name) ? new RegionInfo(culture.Name) : null;

            return
                $"{region?.CurrencyEnglishName} {isoCurrencyCode} {FormattedCurrencyString(DefaultDescriptionAmount, false, culture)}".Trim();
        }

        /// <summary>
        /// Gets the formatted currency description for the specified region in Operator Culture
        /// </summary>
        /// <param name="culture">the culture</param>
        /// <param name="isoCurrencyCode">currency code</param>
        /// <param name="region">the region</param>
        /// <returns></returns>
        public static string GetFormattedDescriptionForOperator(
            this CultureInfo culture,
            string isoCurrencyCode,
            RegionInfo region = null)
        {
            region ??= !string.IsNullOrEmpty(culture.Name) ? new RegionInfo(culture.Name) : null;

            return
                $"{region?.CurrencyEnglishName} {isoCurrencyCode} {FormattedCurrencyStringForOperator(DefaultDescriptionAmount)}".Trim();
        }
        /// <summary>
        /// Apply no currency format on the culture
        /// </summary>
        /// <param name="currencyCulture"></param>
        /// <param name="format"></param>
        public static void ApplyNoCurrencyFormat(this CultureInfo currencyCulture, NoCurrencyFormat format)
        {
            currencyCulture.NumberFormat.CurrencySymbol = string.Empty;
            currencyCulture.NumberFormat.CurrencyGroupSeparator = format.GroupSeparator;
            if (!string.IsNullOrEmpty(format.DecimalSeparator))
            {
                currencyCulture.NumberFormat.CurrencyDecimalSeparator = format.DecimalSeparator;
                currencyCulture.NumberFormat.CurrencyDecimalDigits = 2;
            }
            else
            {
                currencyCulture.NumberFormat.CurrencyDecimalDigits = 0;
            }
        }

        /// <summary>
        /// Update the Currency Culture manually
        /// </summary>
        public static void UpdateCurrencyCulture()
        {
            CurrencyCultureInfo = CultureInfo.CurrentCulture;
        }
    }
}