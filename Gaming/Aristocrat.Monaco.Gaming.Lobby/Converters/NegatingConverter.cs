namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Converts to the correct negative value when scrolling from right to left
    /// </summary>
    public class NegatingConverter : IValueConverter
    {
        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return -d;
            }

            return value;
        }

        /// <inheritdoc />
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return +d;
            }

            return value;
        }
    }
}
