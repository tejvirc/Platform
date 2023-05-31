namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class LobbyColumnMinWidthConverter : IMultiValueConverter
    {
        private const double DefaultMinWidth = 150;
        private const double SmallMinWidth = 100;
        private const double ExtraSmallMinWidth = 50;
        private const double TinyMinWidth = 25;

        public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 3 &&
                int.TryParse(values[0].ToString(), out var gameCount) &&
                bool.TryParse(values[1].ToString(), out bool maintainMargins) &&
                bool.TryParse(values[2].ToString(), out bool isExtraLargeGameIconsActive))
            {
                if (isExtraLargeGameIconsActive)
                {
                    return 0;
                }

                if (maintainMargins)
                {
                    return DefaultMinWidth;
                }

                return gameCount > 18 ? TinyMinWidth : gameCount > 15 ? ExtraSmallMinWidth : gameCount > 12 ? SmallMinWidth : DefaultMinWidth;
            }

            return DefaultMinWidth;
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}