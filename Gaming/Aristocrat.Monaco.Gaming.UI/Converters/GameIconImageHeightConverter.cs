namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Forms;

    public class GameIconImageHeightConverter : IValueConverter
    {
        private const double IconHeight = 200;
        private const double BaseScreenHeight = 1080;
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs)
            {
                var scaleBy = inputs.ScreenHeight / BaseScreenHeight;
                // Lobby Icon ImageHeight. Affects the height of the icon art
                var size = inputs.TabView
                    ? inputs.ExtraLargeIconLayout
                        ? inputs.GameIconSize.Height : IconHeight * scaleBy 
                    : double.NaN;
                if (inputs.GameCount > 8 && inputs.SubTabVisible)
                {
                    // If there are sub tabs, it means we will have less space on the screen
                    // so we have to reduce the image height a bit
                    size -= 30;
                }
                return size;
            }

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
