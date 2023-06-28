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
        /// <param name="value">The decimal value to check</param>
        /// <param name="numOfDecimalPlaces">The number of digits to check for</param>
        /// <returns></returns>
        public static bool CheckPrecision(this decimal value, int numOfDecimalPlaces)
        {
            var decimalAsString = value.ToString(CultureInfo.InvariantCulture);
            var digitsToRightOfDecimal = decimalAsString.SkipWhile(c => c != '.').Skip(1);
            return digitsToRightOfDecimal.Count() == numOfDecimalPlaces;
        }
    }
}