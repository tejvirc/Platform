namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class ScaleByResolutionConverter : IValueConverter
    {
        private readonly double _scaleBy = ScaleUtility.GetScale();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is Thickness valueThickness)
            {
                return new Thickness(
                    valueThickness.Left * _scaleBy,
                    valueThickness.Top * _scaleBy,
                    valueThickness.Right * _scaleBy,
                    valueThickness.Bottom * _scaleBy
                );
            }
            else if (value is double valueDouble)
            {
                return valueDouble * _scaleBy;
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}