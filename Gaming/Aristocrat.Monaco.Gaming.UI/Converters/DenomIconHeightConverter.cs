namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class DenomIconHeightConverter : IValueConverter
    {
        private const double NormalIconHeight = 90;
        private const double LargeIconHeight = 180;
        private const double NormalScreenHeight = 1080;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return NormalIconHeight;
            }

            if (value is double screenHeight)
            {
                // Lobby layout denomination font size based on screen size.
                return screenHeight <= NormalScreenHeight ? NormalIconHeight : LargeIconHeight;
            }

            return NormalIconHeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
