namespace Aristocrat.Monaco.Common.Helpers
{
    using System;
    using System.Globalization;

    /// <summary>
    ///     Various helper functions to aid in working with numbers.
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        ///     Checks that the number of digits to the right of the decimal matches the number of digits specified.
        /// </summary>
        /// <remarks>
        ///     In the case of the decimal value being a Whole Number, this always returns true. This is because precision
        ///     is irrelevant when the decimal value is a non-complex number (integer). e.g. 80.00000 == 80.0.
        /// </remarks>
        /// <param name="value">The decimal value to check</param>
        /// <param name="numOfDecimalPlaces">The number of digits to check for</param>
        /// <returns>True if the decimal value's precision matches the given number of decimal places</returns>
        public static bool CheckPrecision(decimal value, int numOfDecimalPlaces)
        {
            // When the value is a whole number, it matches any given precision
            if (decimal.Floor(value) == value)
            {
                return true;
            }

            var digitsToRightOfDecimal = CountDecimalPlaces(value);

            var precisionMatched = digitsToRightOfDecimal == numOfDecimalPlaces;

            return precisionMatched;
        }

        /// <summary>
        ///     Counts the number of decimal places to the right of the decimal.
        /// </summary>
        /// <param name="value">The value to count decimal places on</param>
        /// <returns>The number of decimal places</returns>
        public static int CountDecimalPlaces(decimal value)
        {
            var precision = 0;

            while (value * (decimal)Math.Pow(10, precision) != Math.Round(value * (decimal)Math.Pow(10, precision)))
            {
                precision++;
            }

            return precision;
        }

        /// <summary>
        ///     Counts the number of digits in the given <c>long</c> number.
        /// </summary>
        /// <remarks>
        ///     This function has been designed verified to cover ALL edge cases, whereas
        ///     other common counting functions will fail. e.g. <c>Math.Abs()</c> throws an
        ///     exception when given long.MinValue.
        /// </remarks>
        /// <param name="number">The long to count the digits for.</param>
        /// <returns>The number of digits.</returns>
        public static int CountDigits(long number)
        {
            return number switch
            {
                0 => 1, // Zero always has 1 digit
                long.MinValue => 19, // long.MinValue has 19 digits
                _ => Math.Abs(number).ToString(CultureInfo.InvariantCulture).Length
            };
        }
    }
}