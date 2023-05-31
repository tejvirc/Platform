namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Models;

    public class GameIconAlignmentConverter : IValueConverter
    {
        private const HorizontalAlignment DefaultHorizontalAlignment = HorizontalAlignment.Left;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is GameGridMarginInputs { ExtraLargeIconLayout: true }
                ? HorizontalAlignment.Center
                : DefaultHorizontalAlignment;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
