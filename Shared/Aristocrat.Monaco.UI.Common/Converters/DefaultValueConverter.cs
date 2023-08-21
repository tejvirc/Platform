namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     When string empty is provided, replace it with a default value.
    /// </summary>
    public class DefaultValueConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is string convertedValue && string.IsNullOrEmpty(convertedValue))
                ? parameter
                : value;
        }
    }
}
