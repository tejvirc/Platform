namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     Converts a number to a visibility value
    /// </summary>
    public class NumberToVisibilityConverter : IValueConverter
    {
        /// <summary>
        ///     Returns Visible if the number is greater than the parameter (or zero if parameter is null), Collapsed otherwise
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var limit = 0.0;
            if (parameter != null)
            {
                double.TryParse(parameter.ToString(), out limit);
            }

            return value is double num && num > limit
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>
        ///     Not used
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}