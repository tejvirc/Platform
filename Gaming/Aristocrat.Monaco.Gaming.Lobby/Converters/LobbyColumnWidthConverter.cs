namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class LobbyColumnWidthConverter : IMultiValueConverter
    {
        private const double DefaultColumnWidth = 8; // 80%
        private const double LargeColumnWidth = 18; // 90%
        private const double ExtraLargeColumnWidth = 38; // 95%

        public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
        {
            var columnSize = DefaultColumnWidth;
            if (values != null && values.Length == 2 &&
                int.TryParse(values[0].ToString(), out var gameCount) &&
                bool.TryParse(values[1].ToString(), out bool maintainMargins) &&
                !maintainMargins)
            {
                columnSize = gameCount > 15 ? ExtraLargeColumnWidth : gameCount > 12 ? LargeColumnWidth : DefaultColumnWidth;
            }

            return new GridLength(columnSize, GridUnitType.Star);
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
