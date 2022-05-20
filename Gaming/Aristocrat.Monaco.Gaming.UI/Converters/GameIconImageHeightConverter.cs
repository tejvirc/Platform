namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class GameIconImageHeightConverter : IValueConverter
    {
        private const double IconHeight = 200;
        private const double LargeScreenSize = 1080;
        private const double LargeScreenScale = 2.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs)
            {
                // Lobby Icon ImageHeight. Affects the height of the icon art
                var size = inputs.TabView
                    ? inputs.ExtraLargeIconLayout
                        ? inputs.GameIconSize.Height : IconHeight 
                    : double.NaN;
                return inputs.ScreenHeight > LargeScreenSize ? size * LargeScreenScale : size;
            }

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
