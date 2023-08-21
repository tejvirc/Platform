namespace Aristocrat.Monaco.Application.Contracts.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Common;
    using Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     TicketCurrencyExtensions
    /// </summary>
    public static class TicketCurrencyExtensions
    {
        private const string And = " and ";
        private const string Space = " ";
        private const decimal OneTrillion = 1.00m * 1000m * 1000m * 1000m * 1000m;
        private const int MillicentPerCentsDigits = 3;

        /// <summary>
        ///     Gets the currency culture info give the player ticket locale
        /// </summary>
        private static readonly Dictionary<string, CurrencyCultureData> PlayerTicketToCurrencyCultureDataMap =
            new Dictionary<string, CurrencyCultureData>();

        /// <summary>
        ///     Gets/Sets the active player ticket locale
        /// </summary>
        public static string PlayerTicketLocale { get; set; } = string.Empty;

        /// <summary>
        ///     Gets Description of the current currency.
        /// </summary>
        public static CurrencyCultureData PlayerTicketCurrencyCultureData =>
            PlayerTicketToCurrencyCultureDataMap[PlayerTicketLocale];

        /// <summary>
        ///     Formats currency string.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <param name="withMillicents">Whether to add extra digits for fractional currency (default false)</param>
        /// <returns>Formatted currency string.</returns>
        public static string FormattedCurrencyStringForVouchers(this decimal amount, bool withMillicents = false)
        {
            var subUnitDigits = PlayerTicketCurrencyCultureData.ValueCultureInfo.NumberFormat.CurrencyDecimalDigits;
            if (withMillicents)
            {
                subUnitDigits += MillicentPerCentsDigits;
            }

            return amount.ToString($"C{subUnitDigits}", PlayerTicketCurrencyCultureData.ValueCultureInfo);
        }

        /// <summary>
        ///     Gets the currency name
        /// </summary>
        /// <param name="locale">The locale</param>
        /// <returns>String of the currency name if found</returns>
        public static string GetCurrencyName(string locale)
        {
            var region = new RegionInfo(locale);

            var currencyNameArray = region.CurrencyEnglishName.Split(' ');

            if (currencyNameArray.Length > 0)
            {
                return currencyNameArray[currencyNameArray.Length - 1];
            }

            return string.Empty;
        }

        /// <summary>
        ///     Set culture information for the currency string.
        /// </summary>
        /// <param name="playerTicketLocale">The player ticket locale this currency is associated with.</param>
        /// <param name="valueCultureInfo">CultureInfo for the numerical value</param>
        /// <param name="wordsCultureInfo">CultureInfo for the numerical words</param>
        /// <param name="minorUnits">Minor currency units.</param>
        /// <param name="minorUnitsPlural">Minor currency units plural form.</param>
        /// <param name="pluralizeMajorUnits">Flag used to set the major units plural form.</param>
        /// <param name="pluralizeMinorUnits">Flag used to set the minor units plural form.</param>
        /// <param name="minorUnitSymbol">Minor Unit Symbol</param>
        public static void SetCultureInfo(
            string playerTicketLocale,
            CultureInfo valueCultureInfo,
            CultureInfo wordsCultureInfo,
            string minorUnits = null,
            string minorUnitsPlural = null,
            bool pluralizeMajorUnits = false,
            bool pluralizeMinorUnits = false,
            string minorUnitSymbol = null)
        {
            var cultureData = new CurrencyCultureData(
                valueCultureInfo,
                wordsCultureInfo,
                string.IsNullOrEmpty(minorUnits)
                    ? Resources.Cent
                    : minorUnits,
                minorUnitsPlural,
                GetCurrencyName(valueCultureInfo.Name),
                pluralizeMajorUnits,
                pluralizeMinorUnits,
                minorUnitSymbol,
                GetCurrencyName(valueCultureInfo.Name));

            cultureData.MinorUnitsPlural = pluralizeMinorUnits
                ? cultureData.MinorUnits.PluralizeWord()
                : cultureData.MinorUnits;

            if (!string.IsNullOrEmpty(cultureData.CurrencyName))
            {
                cultureData.MajorUnitsPlural = pluralizeMajorUnits
                    ? cultureData.CurrencyName.PluralizeWord()
                    : cultureData.CurrencyName;
            }

            PlayerTicketToCurrencyCultureDataMap[playerTicketLocale] = cultureData;
        }

        /// <summary>
        /// Set culture information for the currency string.
        /// </summary>
        /// <param name="playerTicketLocale">ticket locale</param>
        /// <param name="cultureInfo">currency culture info</param>
        /// <param name="currencyName">Currency name</param>
        public static void SetCultureInfo(
            string playerTicketLocale,
            CultureInfo cultureInfo,
            string currencyName)
        {
            var cultureData = new CurrencyCultureData(
                cultureInfo,
                cultureInfo,
                string.Empty,
                String.Empty,
                currencyName,
                false,
                false,
                string.Empty,
                currencyName);
            PlayerTicketToCurrencyCultureDataMap[playerTicketLocale] = cultureData;
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
                throw new ArgumentOutOfRangeException(nameof(amount), @"Argument must be positive");
            }

            if (amount >= OneTrillion)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), @"Amount must be smaller than one trillion");
            }

            var majorUnits = (long)amount;

            var minorUnits = (int)((amount - majorUnits) * 100);

            var textualAmount = majorUnits == 1
                ? $"{majorUnits.NumberToWords(PlayerTicketCurrencyCultureData.WordsCultureInfo)} {PlayerTicketCurrencyCultureData.CurrencyName}"
                : $"{majorUnits.NumberToWords(PlayerTicketCurrencyCultureData.WordsCultureInfo)} {PlayerTicketCurrencyCultureData.MajorUnitsPlural}";

            if (PlayerTicketCurrencyCultureData.WordsCultureInfo.NumberFormat.CurrencyDecimalDigits != 0)
            {
                var minorUnitsText = minorUnits == 1
                    ? PlayerTicketCurrencyCultureData.MinorUnits
                    : PlayerTicketCurrencyCultureData.MinorUnitsPlural;

                textualAmount =
                    $"{textualAmount.Replace(And, Space)} {Localizer.For(CultureFor.Player).GetString(PlayerTicketCurrencyCultureData.WordsCultureInfo, ResourceKeys.And)} {minorUnits.NumberToWords(PlayerTicketCurrencyCultureData.WordsCultureInfo)} {minorUnitsText}";
            }

            return toUpper ? textualAmount.ToUpper(PlayerTicketCurrencyCultureData.WordsCultureInfo) : textualAmount;
        }

        /// <summary>This function will convert a currency to its line-wrapped word representation.</summary>
        /// <param name="amount">The amount.</param>
        /// <param name="lineLength">The line length</param>
        /// <returns>
        ///     A list of wrapped lines of text to add to a ticket. There will be at least 2 lines, even
        ///     if the second line is empty.
        /// </returns>
        public static IList<string> ConvertCurrencyToWrappedWords(decimal amount, int lineLength)
        {
            var result = ConvertCurrencyToWords(amount);
            return result.ConvertStringToWrappedWords(lineLength);
        }
    }
}