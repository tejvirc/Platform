namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     If the character is a comma, shift it down a bit
    /// </summary>
    internal class CommaMarginConverter : IValueConverter
    {
        private const string CommaSymbol = ",";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return new Thickness();
            }

            var character = ((char)value).ToString();
            var marginToApply = (Thickness)parameter;

            if (character.Equals(CommaSymbol, StringComparison.Ordinal))
            {
                return marginToApply;
            }

            return new Thickness();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

