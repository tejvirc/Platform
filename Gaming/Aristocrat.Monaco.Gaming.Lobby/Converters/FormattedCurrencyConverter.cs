namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Application.Contracts.Extensions;

    /// <summary>
    ///     FormattedCurrencyConverter
    /// </summary>
    /// <remarks>
    ///     This is for converting millicents to a formatted currency value
    ///     If you pass in a converter Parameter of ZeroNotAvailable, then 0 will be mapped to N/A
    /// </remarks>
    public class FormattedCurrencyConverter : IValueConverter
    {
        private const string ZeroNotAvailable = "ZeroNotAvailable";
        private const string NotAvailable = "N/A";
        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is long currency)
            {
                if (currency == 0 && parameter is string param && param == ZeroNotAvailable)
                {
                    return NotAvailable;
                }
                return currency.MillicentsToDollars().FormattedCurrencyString();
            }

            return string.Empty;
        }

        /// <inheritdoc />
        /// <exception cref="NotImplementedException">Thrown when the requested operation is unimplemented.</exception>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
