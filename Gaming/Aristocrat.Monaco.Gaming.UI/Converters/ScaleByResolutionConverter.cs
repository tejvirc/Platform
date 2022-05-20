namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Forms;

    public class ScaleByResolutionConverter : IValueConverter
    {
        private const double BaseScreenWidth = 1920;
        private readonly double _scaleBy = Screen.PrimaryScreen.Bounds.Width / BaseScreenWidth;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}