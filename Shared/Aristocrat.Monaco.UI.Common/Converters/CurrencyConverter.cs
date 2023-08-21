namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Application.Contracts.Extensions;

    /// <summary>
    /// CurrencyConverter
    /// </summary>
    public class CurrencyConverter : IValueConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is decimal decimalValue)
                {
                    return decimalValue.FormattedCurrencyString();
                }

                if (value is double doubleValue)
                {
                    return doubleValue.FormattedCurrencyString();
                }

                if (value is long longValue)
                {
                    return longValue.FormattedCurrencyString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
