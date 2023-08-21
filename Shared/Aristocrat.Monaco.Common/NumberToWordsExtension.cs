namespace Aristocrat.Monaco.Common
{
    using System.Globalization;
    using Humanizer;

    /// <summary>
    ///     A number to words extension.
    /// </summary>
    public static class NumberToWordsExtension
    {
        /// <summary>
        ///     Get number value to words string.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <param name="culture">Culture Info.</param>
        /// <returns>Number to words string.</returns>
        public static string NumberToWords(this long amount, CultureInfo culture)
        {
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }

            return amount.ToWords(culture);
        }

        /// <summary>
        ///     Get number value to words string.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <param name="culture">Culture Info.</param>
        /// <returns>Number to words string.</returns>
        public static string NumberToWords(this int amount, CultureInfo culture)
        {
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }

            return amount.ToWords(culture);
        }

        /// <summary>
        ///     Pluralizes a string value.
        /// </summary>
        /// <param name="word">Word to pluralize.</param>
        /// <returns>Pluralized word.</returns>
        public static string PluralizeWord(this string word)
        {
            return word.Pluralize();
        }

        /// <summary>
        ///     Returns the ordinal string of the given number.
        /// </summary>
        /// <param name="num">Number to get ordinal of.</param>
        /// <param name="culture">Culture Info.</param>
        /// <returns>Ordinal of number</returns>
        public static string ToOrdinal(this int num, CultureInfo culture)
        {
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }

            return num.ToOrdinalWords(culture);
        }
    }
}