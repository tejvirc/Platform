namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Application.Contracts.Extensions;

    /// <summary>
    ///     Cents to decimal converter.
    /// </summary>
    /// <remarks>
    ///     This will typically be used for converting cents to a currency value.
    ///     Formatting should applied by the view.
    /// </remarks>
    public class CentsToDecimalConverter : IValueConverter
    {
        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is long converted)
            {
                return converted.CentsToDollars();
            }

            return 0m;
        }

        /// <inheritdoc />
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal converted)
            {
                return converted.DollarsToCents();
            }

            return 0m;
        }
    }
}
