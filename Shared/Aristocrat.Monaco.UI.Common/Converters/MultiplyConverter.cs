namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Multiplies input double values and returns the product
    /// </summary>
    public class MultiplyConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Convert a list of input double values to a product
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 1.0;
            foreach (var value in values)
            {
                if (value is double d)
                    result *= d;
            }

            return result;
        }

        /// <summary>
        ///     Not used
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
    }
}
