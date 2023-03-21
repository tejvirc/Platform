namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class GameIconImageMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is GameGridMarginInputs { ExtraLargeIconLayout: false, GameWindowHeight: > ScaleUtility.BaseScreenHeight, GameCount: > 8 } ?
                // Unfortunately the game icon is not created properly, the icons have some blank spaces above and below
                // the images, so we have to move the image up a bit to display it in correct place.
                new Thickness(0, -40, 0, 0) : new Thickness();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
