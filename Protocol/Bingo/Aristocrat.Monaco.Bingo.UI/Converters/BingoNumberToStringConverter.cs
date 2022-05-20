namespace Aristocrat.Monaco.Bingo.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Converts bool to string property.  By setting the TrueValue
    ///     and False value properties, the string mappings can be customized.
    /// </summary>
    public class BingoNumberToStringConverter : IValueConverter
    {
        private const string CardCenterText = " * ";
        private const string CardNumberUnknown = " ? ";

        /// <summary>
        ///     Convert from bingo ball number to a string
        /// </summary>
        /// <param name="value">the numeric value to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>number as a string or symbol</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number)
            {
                return number == 0 ? CardCenterText : number.ToString("D2");
            }

            return CardNumberUnknown;
        }

        /// <summary>
        ///     Convert string to bingo ball number
        /// </summary>
        /// <param name="value">the true/false string to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>not used</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
