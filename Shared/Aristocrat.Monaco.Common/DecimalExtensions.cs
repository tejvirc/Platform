namespace Aristocrat.Monaco.Common
{
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     Various extensions to aid in working with decimal numbers.
    /// </summary>
    public static class DecimalExtensions
    {
        /// <summary>
        ///     Checks that the number of digits to the right of the decimal matches the number of digits specified.
        /// </summary>
        /// <remarks>
        ///     In the case of the decimal value being a Whole Number, this always returns true. This is because precision
        ///     is irrelevant when the decimal value is a non-complex number (integer).
        /// </remarks>
        /// <param name="value">The decimal value to check</param>
        /// <param name="numOfDecimalPlaces">The number of digits to check for</param>
        /// <returns>True if the decimal value's precision matches the given number of decimal places</returns>
        public static bool CheckPrecision(this decimal value, int numOfDecimalPlaces)
        {
            if (decimal.Floor(value) == value)
            {
                return true;
            }

            var decimalAsString = value.ToString(CultureInfo.InvariantCulture);
            var digitsToRightOfDecimal = decimalAsString.SkipWhile(c => c != '.').Skip(1);

            return digitsToRightOfDecimal.Count() == numOfDecimalPlaces;
        }
    }
}